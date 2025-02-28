<template>
    <!-- Container for Bottom Colored Toasts -->
    <div class="toast-container position-fixed bottom-0 end-0 p-3">

        <!-- General Error Message Dialog -->
        <div id="liveToast-ErrMsg" class="toast align-items-center bg-danger border-0" role="alert" aria-live="assertive" aria-atomic="true" style="width: 650px;" v-on:click="toastClicked('ErrMsg')">
            <div class="d-flex align-items-center" style="height: 100%;">
                <i class="bi bi-bug" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
                <div class="toast-body" >
                    <h5>{{ toasts.ErrMsg.title }}</h5>
                    <p v-html="toasts.ErrMsg.message"></p>
                    <pre class="bg-danger" v-html="toasts.ErrMsg.stack" style="width: 550px;"></pre>
                    <div class="btn-group" role="group" aria-label="Basic outlined example">
                        <button type="button" class="btn btn-outline-light" @click="copyToClipboard(toasts.ErrMsg.message + '\n\n' + toasts.ErrMsg.stack)">Copy Error</button>
                        <button type="button" class="btn btn-outline-light" data-bs-dismiss="toast" aria-label="Close">Close</button>
                    </div>            
                </div>
            </div>
        </div>

        <!-- Error Type Toast Notification -->
        <div id="liveToast-Error" class="toast align-items-center bg-danger border-0" role="alert" aria-live="assertive" aria-atomic="true" v-on:click="toastClicked('Error')">
            <div class="d-flex align-items-center" style="height: 100%;">
                <i class="bi bi-exclamation-octagon" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
                <div class="toast-body" v-html="toasts.Error.message"></div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>

        <!-- Success Type Toast Notification -->
        <div id="liveToast-Success" class="toast align-items-center bg-success border-0" role="alert" aria-live="assertive" aria-atomic="true" v-on:click="toastClicked('Success')">
            <div class="d-flex align-items-center" style="height: 100%;">
                <i class="bi bi-check-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
                <div class="toast-body" v-html="toasts.Success.message"></div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>

        <!-- Warning Type Toast Notification -->
        <div id="liveToast-Warning" class="toast align-items-center bg-warning border-0" role="alert" aria-live="assertive" aria-atomic="true" v-on:click="toastClicked('Warning')">
            <div class="d-flex align-items-center" style="height: 100%;">
                <i class="bi bi-exclamation-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
                <div class="toast-body text-black" v-html="toasts.Warning.message"></div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>

        <!-- Info Type Toast Notification -->
        <div id="liveToast-Info" class="toast align-items-center bg-info border-0" role="alert" aria-live="assertive" aria-atomic="true" v-on:click="toastClicked('Info')">
            <div class="d-flex align-items-center" style="height: 100%;">
                <i class="bi bi-info-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
                <div class="toast-body text-black" v-html="toasts.Info.message"></div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>

    </div> <!-- Container for Bottom Colored Toasts -->
</template>

<script>
import EventBus from '../../EventBus';
export default {
    name: 'Notifications',
    components: {},
    data() {
        return {
            toasts: {
                Error:      { title: '', message: '' },
                Success:    { title: '', message: '' },
                Warning:    { title: '', message: '' },
                Info:       { title: '', message: '' },
                ErrMsg:     { title: '', message: '', stack: '' },
            },
        }
    },
    methods: {

        /** Displays a Colored Toast Notification at Bottom Right corner of the Window
         * @param data Configuration Object: 
         * { 
         *      type: 'Info', //<- Info, Success, Warning, Error
         *      title: '', 
         *      message: '',
         *      stack: 'Stack Trace for Errors',
         *      autoHide: true
         * }         */
         showToast(data) {
            const { type, title, message, stack, autoHide = true } = data;
            const toastType = type.charAt(0).toUpperCase() + type.slice(1);

            try {
                if (this.toasts[toastType]) {
                    this.toasts[toastType].title = title;
                    this.toasts[toastType].message = message;
                    if (stack) this.toasts[toastType].stack = stack;

                    const toast = document.getElementById(`liveToast-${toastType}`);
                    let toastBootstrap = bootstrap.Toast.getInstance(toast);

                    const options = {
                        autohide: autoHide,
                        delay: 3000 // Auto-hide delay in milliseconds
                    };

                    if (toastBootstrap) {
                        toastBootstrap.dispose();
                    }
                    toastBootstrap = new bootstrap.Toast(toast, options);
                    toastBootstrap.show();

                    if (toastType === 'ErrMsg') {
                        window.api.logError(error.message, error.stack);
                    }
                } else {
                    console.error(`Toast type "${type}" not recognized.`);
                }
            } catch (error) {
                console.log(error.message + error.stack);
            } 
        },

        /** Displays an Error Toast Notification at Bottom Right corner of the Window
         * @param {Error} error - The error object to display
         */
        showError(error) {
            try {
                const toastType = 'ErrMsg';

                if (this.toasts[toastType]) {

                    this.toasts[toastType].title = 'Unexpected Error';
                    this.toasts[toastType].message = error.message;
                    this.toasts[toastType].stack = error.stack;

                    const toast = document.getElementById(`liveToast-${toastType}`);
                    let toastBootstrap = bootstrap.Toast.getInstance(toast);

                    if (toastBootstrap) {
                        toastBootstrap.show();
                    } else {
                        toastBootstrap = new bootstrap.Toast(toast, { autohide: false });
                        toastBootstrap.show();
                    }
                    console.error(error.message, error.stack);

                } else {
                    console.error(`Toast type "${type}" not recognized.`);
                }
            } catch (error) {
                console.log(error.message + error.stack);
            }
        },

        /** Closes a toast programmatically
         * @param {String} toastType - The type of toast to close
         */
         closeToast(toastType) {
            const toast = document.getElementById(`liveToast-${toastType}`);
            const toastBootstrap = bootstrap.Toast.getInstance(toast);
            if (toastBootstrap) {
                toastBootstrap.hide();
            }
        },

        /** Handles click events on toasts
         * @param {String} toastType - The type of toast that was clicked
         */
        toastClicked(toastType) {
            //console.log(`${toastType} toast clicked!`);
            this.closeToast(toastType);
            // You can perform additional actions here when a toast is clicked
        },

        copyToClipboard(msg){
            window.api.copyToClipboard(msg);
        }
  
    },
    async mounted() {
        /* EVENTS WE LISTEN TO HERE:  */
        EventBus.on('RoastMe', this.showToast);
        EventBus.on('ShowError', this.showError);
        EventBus.on('closeToast', this.closeToast);
    },
    beforeUnmount() {
        // Clean up the event listener
        EventBus.off('RoastMe', this.showToast);
        EventBus.off('ShowError', this.showError);
        EventBus.off('closeToast', this.closeToast);
    },
}
</script>


<style scoped>
.visually-hidden {
  color: #ffffff;
}

.toast-body {
  white-space: pre-wrap;
  display: flex;
  flex-direction: column;
  align-items: start;
}

.toast-body h5 {
  margin: 0;
}

.toast-body pre {
  background-color: #2c2c2c;
  color: #ffffff;
  padding: 10px;
  border-radius: 5px;
  overflow-x: auto;
}
</style>