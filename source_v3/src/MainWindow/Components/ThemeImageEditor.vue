
<template>
    <div class="modal fade show" id="themeImageEditorModal" tabindex="-1" aria-labelledby="themeImageEditorModalLabel" aria-hidden="true" style="display: block;">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="themeImageEditorModalLabel">{{ themeName }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" @click="$emit('close')"></button>
          </div>
          <div class="modal-body">
            <div id="imgEditorAlert"></div>
            <div class="row mb-3 preview-row">
              <div class="col">

                <div class="image-container preview-div" @paste="handlePaste" @dragover="allowDrop" @drop="handleDrop" @dblclick="openFileDialog">
                    <img id="previewImage" ref="previewImage" :src="previewImage" alt="Preview Image" v-show="previewImage" />
                    <div id="phPreview" class="placeholder placeholder-wave bg-secondary" v-show="!previewImage">
                        Preview Area<br>
                        Paste an in-game screenshot here<br>
                        Or Double Click to Open an existing image.<br><br>
                        Waiting for input..
                    </div>
                    <div id="cropArea" ref="cropArea" v-show="previewImage" class="crop-area">
                        <div class="resize-handle top-left" @mousedown="startResizing('top-left', $event)"></div>
                        <div class="resize-handle top-right" @mousedown="startResizing('top-right', $event)"></div>
                        <div class="resize-handle bottom-left" @mousedown="startResizing('bottom-left', $event)"></div>
                        <div class="resize-handle bottom-right" @mousedown="startResizing('bottom-right', $event)"></div>
                        <div class="resize-handle top" @mousedown="startResizing('top', $event)"></div>
                        <div class="resize-handle bottom" @mousedown="startResizing('bottom', $event)"></div>
                        <div class="resize-handle left" @mousedown="startResizing('left', $event)"></div>
                        <div class="resize-handle right" @mousedown="startResizing('right', $event)"></div>
                    </div>
                </div> <!--/Preview--> 

              </div>
            </div>

            <div class="row mb-3 thumbnail-row">
              <div class="col">
                <div class="image-container">
                    <img id="thumbnailImage" :src="thumbnailImage" alt="Thumbnail Image" v-show="thumbnailImage" class="img-fluid" />
                    <div id="phThumbnail" class="placeholder" v-show="!thumbnailImage">
                        Thumbnail will be generated from Preview
                    </div>
                </div>
              </div>
            </div><!--/Thumbnail-->

            <!-- TOAST NOTIFICATION -->
            <div class="toast-container position-fixed bottom-0 start-0 p-3">
                <!-- Info Type Toast Notification -->
                <div id="Toast-Info" class="toast align-items-center bg-info border-0" role="alert" aria-live="assertive" aria-atomic="true" style="width: 550px;">
                    <div class="d-flex align-items-center" style="height: 100%;">
                        <i class="bi bi-info-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
                        <div class="toast-body text-black">
                            <b>Editor for Theme Images:</b><br>
                            <ul>
                                <li>Copy/Paste an Screenshot showing your Theme.</li>
                                <li>Can Double click the Image to Open an existing Image.</li>
                                <li>Supported Images: 'jpg', 'jpeg', 'png', 'gif'.</li>
                                <li>Use the Red area to fine-tune your Thumbnail boundaries.</li>
                                <li>Click Generate and Save buttons.</li>
                            </ul>
                        </div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                    </div>
                </div>
            </div> <!--/Toast-->

         </div> <!-- /Body -->
        <div class="modal-footer">
            <div class="btn-group" role="group" >
                <button type="button" class="btn btn-secondary" @click="showInfo">Info / Help</button>
                <button type="button" class="btn btn-secondary" @click="clearImages">Clear Images</button>
                <button type="button" class="btn btn-info" @click="generateThumbnail">Generate Thumbnail</button>
                <button type="button" class="btn btn-success" @click="saveImages">Save</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </template> 

<script>

/** To Check is something is Empty
 * @param obj Object to check
 */
 const isEmpty = obj => Object.keys(obj).length === 0;

export default {
    props: {
        themeEditorData: {
            theme: '',
            author: '',
            description: '',
            preview: null,
            thumb: null
        }
    },
    data() {
        return {
            themeMeta: null,
            themeName: this.themeEditorData.theme,

            previewImage: this.themeEditorData.preview,
            thumbnailImage: this.themeEditorData.thumb,

            cropStartX: 0,
            cropStartY: 0,
            cropEndX: 100,
            cropEndY: 100,

            isResizing: false,
            resizeDirection: null,            
        };
    },
    methods: {
        Initialize() {
            const img = new Image();
            img.src = this.previewImage;
            img.onload = () => {
                this.$nextTick(() => {
                    this.initializeCropArea();
                });
            };
        },
        initializeCropArea() {
            //const cropArea = this.$refs.cropArea; console.log(cropArea);
            const cropArea = document.getElementById('cropArea'); //console.log(cropArea);            
            if (cropArea) {
                const parentRect = cropArea.parentElement.getBoundingClientRect();
                this.cropStartX = 30;
                this.cropStartY = parentRect.height - 130; // Position at the bottom
                this.cropEndX = parentRect.width - 30;
                this.cropEndY = parentRect.height - 20;

                cropArea.style.left = `${this.cropStartX}px`;
                cropArea.style.top = `${this.cropStartY}px`;
                cropArea.style.width = `${this.cropEndX - this.cropStartX}px`;
                cropArea.style.height = `${this.cropEndY - this.cropStartY}px`;
                cropArea.addEventListener('mousedown', this.startDragging);
            }

            document.addEventListener('mousemove', this.dragCropArea);
            document.addEventListener('mouseup', this.stopDragging);
        },

        allowDrop(event) {
            event.preventDefault();
            event.dataTransfer.effectAllowed = 'copy'; // or 'move' depending on your needs
        },

        async openFileDialog() {
            try {
                const options = {
                    title: 'Select an Screenshot Image:',
                    filters: [
                        { name: 'Images', extensions: ['jpg', 'jpeg', 'png', 'gif'] }
                    ]
                }; 
                const imgPath = await window.api.ShowOpenDialog(options); // console.log('imgPath', imgPath[0]);                
                if (imgPath && !isEmpty(imgPath)) {
                    const Image  = await window.api.GetImageB64(imgPath[0]); // `file://${imgPath}`;                   
                    //console.log('Image Data: ', Image);
                    this.$nextTick(() => {
                        this.previewImage = Image;
                        this.initializeCropArea();
                    });
                }
            } catch (error) {
                console.error('Error opening file dialog:', error);
            }
        },
        handleDrop(event) {
            try {
                event.preventDefault();
                const file = event.dataTransfer.files[0];
                const reader = new FileReader();

                reader.onload = (e) => {
                    this.previewImage = e.target.result;
                    this.$nextTick(() => {
                        this.initializeCropArea();
                    });
                };

                reader.readAsDataURL(file);
            } catch (error) {
                console.error(error);
            }
        },
        handlePaste(event) {
            try {
                event.preventDefault();
                console.log('Pasting..', event);
                const items = (event.clipboardData || window.clipboardData).items;
                for (let item of items) {
                    if (item.type.indexOf('image') !== -1) {
                        const blob = item.getAsFile();
                        const reader = new FileReader();
                        reader.onload = (e) => {
                            const Image  = e.target.result;
                            this.$nextTick(() => {
                                this.previewImage = Image; // Load the new image
                                this.initializeCropArea();
                            });
                        };
                        reader.readAsDataURL(blob);
                    }
                }
            } catch (error) {
                console.error(error);
            }
        },
        
        startDragging(event) {
            this.isDragging = true;
            this.cropStartX = event.offsetX;
            this.cropStartY = event.offsetY;
        },
        dragCropArea(event) {
            if (!this.isDragging) return;
            const cropArea = this.$refs.cropArea;
            const rect = cropArea.parentElement.getBoundingClientRect();
            this.cropEndX = event.clientX - rect.left;
            this.cropEndY = event.clientY - rect.top;
            cropArea.style.width = `${this.cropEndX - this.cropStartX}px`;
            cropArea.style.height = `${this.cropEndY - this.cropStartY}px`;
        },
        stopDragging() {
            this.isDragging = false;
        },


        startResizing(direction, event) {
            this.isResizing = true;
            this.resizeDirection = direction;
            event.stopPropagation();
        },
        resizeCropArea(event) {
            if (!this.isResizing) return;
            const cropArea = this.$refs.cropArea;
            const rect = cropArea.parentElement.getBoundingClientRect();
            switch (this.resizeDirection) {
                case 'top-left':
                    this.cropStartX = event.clientX - rect.left;
                    this.cropStartY = event.clientY - rect.top;
                    break;
                case 'top-right':
                    this.cropEndX = event.clientX - rect.left;
                    this.cropStartY = event.clientY - rect.top;
                    break;
                case 'bottom-left':
                    this.cropStartX = event.clientX - rect.left;
                    this.cropEndY = event.clientY - rect.top;
                    break;
                case 'bottom-right':
                    this.cropEndX = event.clientX - rect.left;
                    this.cropEndY = event.clientY - rect.top;
                    break;
                case 'top':
                    this.cropStartY = event.clientY - rect.top;
                    break;
                case 'bottom':
                    this.cropEndY = event.clientY - rect.top;
                    break;
                case 'left':
                    this.cropStartX = event.clientX - rect.left;
                    break;
                case 'right':
                    this.cropEndX = event.clientX - rect.left;
                    break;
            }
            cropArea.style.left = `${this.cropStartX}px`;
            cropArea.style.top = `${this.cropStartY}px`;
            cropArea.style.width = `${this.cropEndX - this.cropStartX}px`;
            cropArea.style.height = `${this.cropEndY - this.cropStartY}px`;
        },
        stopResizing() {
            this.isResizing = false;
            this.resizeDirection = null;
        },

        showInfo() {
            const toastLiveExample = document.getElementById('Toast-Info');
            if (toastLiveExample) {
                const toastBootstrap = bootstrap.Toast.getOrCreateInstance(toastLiveExample);
                toastBootstrap.show();
            }
        },

        /** Display an Alert message on the top. * 
         * @param message Text Message to be shown.
         * @param type Color of the Alert -> [primary (Blue), secondary (Dark), success (Green), danger (Red), warning (Orange), info (Cyan), light (White/Grey), dark (Black)]
         */
        showAlert(message, type) {
            const wrapper = document.createElement('div');
            const alertPlaceholder = document.getElementById('imgEditorAlert');
            wrapper.innerHTML = [
                `<div class="alert alert-${type} alert-dismissible" role="alert">`,
                `   <div>${message}</div>`,
                '   <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>',
                '</div>'
            ].join('');
            alertPlaceholder.append(wrapper);
        },

        clearImages() {
            this.previewImage = null;
            this.thumbnailImage = null;
        },

        generateThumbnail() {
            if (!this.previewImage) return;

            const img = new Image();
            img.src = this.previewImage;
            img.onload = () => {
                const cropArea = this.$refs.cropArea;
                const rect = cropArea.getBoundingClientRect();
                const imageRect = this.$refs.previewImage.getBoundingClientRect();

                // Calculate the actual image dimensions within the container
                const scaleX = img.width / imageRect.width;
                const scaleY = img.height / imageRect.height;

                // Calculate the cropping coordinates relative to the image
                const cropX = (rect.left - imageRect.left) * scaleX;
                const cropY = (rect.top - imageRect.top) * scaleY;
                const cropWidth = rect.width * scaleX;
                const cropHeight = rect.height * scaleY;

                // Create a temporary canvas to crop and resize the image directly
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');
                canvas.width = 360;
                canvas.height = 71;
                ctx.imageSmoothingEnabled = true;
                ctx.imageSmoothingQuality = 'high';

                // Draw the cropped and resized image directly on the canvas
                ctx.drawImage(img, cropX, cropY, cropWidth, cropHeight, 0, 0, 360, 71);

                this.thumbnailImage = canvas.toDataURL('image/png');
            };
        },
        saveImages() {
            if (this.thumbnailImage && !isEmpty(this.thumbnailImage)) {
                this.$emit('save', {
                    previewImage: this.previewImage,
                    thumbnailImage: this.thumbnailImage
                });
            } else {
                this.showAlert('The Thumbnail Image is Required', 'warning');
            }
        }
    },

    mounted() {
        this.Initialize();

        document.addEventListener('mousemove', this.resizeCropArea);
        document.addEventListener('mouseup', this.stopResizing);
    },
    beforeDestroy() {
        const cropArea = this.$refs.cropArea;
        if (cropArea) {
            cropArea.removeEventListener('mousedown', this.startDragging);
        }
        document.removeEventListener('mousemove', this.dragCropArea);
        document.removeEventListener('mouseup', this.stopDragging);
        document.removeEventListener('mousemove', this.resizeCropArea);
        document.removeEventListener('mouseup', this.stopResizing);
    }
};
</script>

<style scoped>
.modal-dialog {
    height: 680px;
    width: 700px;
}

.modal-content {
    height: 100%;
}

.modal-body {
    display: flex;
    flex-direction: column;
    height: 100%;
}

.preview-row {
    height: 400px;
    max-height: 400px;
    min-height: 400px;
    /*height: 70%; /* Set a fixed height for the preview row */
}

#previewImage {
    top: 0;
    height: 372px;
    max-height: 372px;
    min-height: 372px;
}

#phPreview {
    height: 100%;
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #000000;
    font-size: 16px;
    text-align: center;
    position: absolute;
    top: 0;
    left: 0;
    background: rgba(255, 255, 255, 0.8);
}

#phThumbnail {
    top: 433px;
    left: 17px;
    height: 78px;
    width: 664px;
}

#thumbnailImage {
    width: 360px;
    min-width: 360px;
    max-width: 360px;
    height: 71px;
    min-height: 71px;
    max-height: 71px;
}

.thumbnail-row {
    height: 80px;
    min-height: 80px;
    max-height: 80px;
    /* height: 30%; /* Set a fixed height for the thumbnail row */
}

.preview-div {
    height: 100%;
    max-height: 100%;
    position: relative;
}

.image-container {
    border: 1px dashed #ccc;
    padding: 10px;
    text-align: center;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    /* Prevent the image from resizing the container */
}

.image-container img {
    max-width: 100%;
    max-height: 100%;
    object-fit: contain;
    /* Ensure the image fits within the container */
}

.crop-area {
    position: absolute;
    border: 2px dashed #ff0000;
    cursor: move;
}

.resize-handle {
    position: absolute;
    width: 10px;
    height: 10px;
    background: #ff0000;
    z-index: 10;
}

.resize-handle.top-left {
    top: -5px;
    left: -5px;
    cursor: nwse-resize;
}

.resize-handle.top-right {
    top: -5px;
    right: -5px;
    cursor: nesw-resize;
}

.resize-handle.bottom-left {
    bottom: -5px;
    left: -5px;
    cursor: nesw-resize;
}

.resize-handle.bottom-right {
    bottom: -5px;
    right: -5px;
    cursor: nwse-resize;
}

.resize-handle.top {
    top: -5px;
    left: 50%;
    transform: translateX(-50%);
    cursor: ns-resize;
}

.resize-handle.bottom {
    bottom: -5px;
    left: 50%;
    transform: translateX(-50%);
    cursor: ns-resize;
}

.resize-handle.left {
    top: 50%;
    left: -5px;
    transform: translateY(-50%);
    cursor: ew-resize;
}

.resize-handle.right {
    top: 50%;
    right: -5px;
    transform: translateY(-50%);
    cursor: ew-resize;
}

.placeholder {
    color: #161515;
    font-size: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    height: 100%;
    width: 100%;
    text-align: center;
    position: absolute;
    top: 0;
    left: 0;
    background: rgba(255, 255, 255, 0.8);
    /* Light background to ensure text visibility */
}
</style>