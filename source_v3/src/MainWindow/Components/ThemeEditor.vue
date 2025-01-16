<!-- filepath: /G:/@Proyectos/EDHM_UI/source_v3/src/MainWindow/Components/ThemeEditor.vue -->
<template>
    <div class="modal fade show" id="themeEditorModal" tabindex="-1" aria-labelledby="themeEditorModalLabel" aria-hidden="true" style="display: block;">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="themeEditorModalLabel">Edit Theme</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" @click="$emit('close')"></button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="submitForm">
              <div class="mb-3">
                <label for="themeName" class="form-label">Name</label>
                <input type="text" class="form-control" id="themeName" v-model="themeName" required>
              </div>
              <div class="mb-3">
                <label for="themeAuthor" class="form-label">Author</label>
                <input type="text" class="form-control" id="themeAuthor" v-model="themeAuthor" required>
              </div>
              <div class="mb-3">
                <label for="themeDescription" class="form-label">Description</label>
                <textarea class="form-control" id="themeDescription" v-model="themeDescription" rows="3" required></textarea>
              </div>
              <div class="mb-3">
                <label for="themeThumbnail" class="form-label">Thumbnail Image</label>
                <img v-if="themeThumbnail" :src="themeThumbnail" alt="Thumbnail Preview" class="img-thumbnail mt-2">
              </div>
              <div class="input-group mb-3">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" @click="$emit('close')">Cancel</button>
                <button type="submit" class="btn btn-primary">Submit</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  </template>
  
<script>
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
            themeName: this.themeEditorData.theme,
            themeAuthor: this.themeEditorData.author,
            themeDescription: this.themeEditorData.description,
            themeThumbnail: this.themeEditorData.thumb,
            themePreview: this.themeEditorData.preview
        };
    },
    methods: {
        submitForm(event) {
            event.preventDefault(); // Prevent the default form submission behavior
            const themeData = {
                credits: {
                    theme: this.themeName,
                    author: this.themeAuthor,
                    description: this.themeDescription,
                    preview: this.themePreview,
                    thumb: this.themeThumbnail
                }
            };
            this.$emit('submit', themeData);
        },
        handleFileChange(event) {
            const file = event.target.files[0];
            const reader = new FileReader();
            reader.onload = (e) => {
                this.resizeImage(e.target.result, 360, 71, (resizedImage) => {
                    this.themeThumbnail = resizedImage;
                });
            };
            reader.readAsDataURL(file);
        },
        resizeImage(base64Str, maxWidth, maxHeight, callback) {
            const img = new Image();
            img.src = base64Str;
            img.onload = () => {
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');
                canvas.width = maxWidth;
                canvas.height = maxHeight;
                ctx.drawImage(img, 0, 0, maxWidth, maxHeight);
                callback(canvas.toDataURL('image/png'));
            };
        },
        
    }
};
</script>
  
  <style scoped>
  .modal-dialog {
    max-width: 500px;
  }
  
  .modal-content {
    padding: 20px;
  }
  
  .form-label {
    font-weight: bold;
  }
  
  .img-thumbnail {
    width: 360px;
    height: 71px;
    object-fit: contain;
  }
  
  .btn-close {
    background: none;
    border: none;
  }
  </style>