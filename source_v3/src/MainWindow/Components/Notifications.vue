<template>
    <!-- Container for Bottom Colored Toasts -->
    <div class="toast-container position-fixed bottom-0 end-0 p-3">

        <!-- General Error Message Dialog -->
        <div id="liveToast-ErrMsg" class="toast align-items-center bg-danger border-0" role="alert"
            aria-live="assertive" aria-atomic="true" style="width: 650px;" v-on:click="toastClicked('ErrMsg')">
            <div class="d-flex align-items-center" style="height: 100%;">
                <i class="bi bi-bug" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
                <div class="toast-body">
                    <h5>{{ toasts.ErrMsg.title }}</h5>
                    <p v-html="toasts.ErrMsg.message"></p>
                    <pre class="bg-danger" v-html="toasts.ErrMsg.stack" style="width: 550px;"></pre>
                    <div class="btn-group" role="group" aria-label="Basic outlined example">
                        <button type="button" class="btn btn-outline-light"
                            @click="copyToClipboard(toasts.ErrMsg.message + '\n\n' + toasts.ErrMsg.stack)">Copy
                            Error</button>
                        <button type="button" class="btn btn-outline-light" data-bs-dismiss="toast"
                            aria-label="Close">Close</button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Error Type Toast Notification -->
        <div id="liveToast-Error" class="toast align-items-center bg-danger border-0" role="alert" aria-live="assertive"
            aria-atomic="true" v-on:click="toastClicked('Error')">
            <div class="d-flex align-items-center" style="height: 100%;">
                <i class="bi bi-exclamation-octagon" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
                <div class="toast-body" v-html="toasts.Error.message"></div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"
                    aria-label="Close"></button>
            </div>
        </div>

        <!-- Success Type Toast Notification -->
        <div id="liveToast-Success" class="toast align-items-center bg-success border-0" role="alert"
            aria-live="assertive" aria-atomic="true" v-on:click="toastClicked('Success')">
            <div class="d-flex align-items-center" style="height: 100%;">
                <i class="bi bi-check-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
                <div class="toast-body" v-html="toasts.Success.message"></div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"
                    aria-label="Close"></button>
            </div>
        </div>

        <!-- Warning Type Toast Notification -->
        <div id="liveToast-Warning" class="toast align-items-center bg-warning border-0" role="alert"
            aria-live="assertive" aria-atomic="true" v-on:click="toastClicked('Warning')">
            <div class="d-flex align-items-center" style="height: 100%;">
                <i class="bi bi-exclamation-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
                <div class="toast-body text-black" v-html="toasts.Warning.message"></div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>

        <!-- Info Type Toast Notification -->
        <div id="liveToast-Info" class="toast align-items-center bg-info border-0" role="alert" aria-live="assertive"
            aria-atomic="true" v-on:click="toastClicked('Info')">
            <div class="d-flex align-items-center" style="height: 100%;">
                <i class="bi bi-info-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
                <div class="toast-body text-black" v-html="toasts.Info.message"></div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>

        <!-- Accent Type Toast Notification -->
        <div id="liveToast-Accent" class="toast custom-toast align-items-start text-bg-primary border-0" role="alert" aria-live="assertive" 
            aria-atomic="true"  v-on:click="toastClicked('Accent')">
            <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            <div class="d-flex w-100" style="height: 100%;">
                <i class="bi bi-info-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
                <div class="toast-body" >
                    <div class="toast-title fw-bold mb-1" v-html="toasts.Accent.title"></div>
                    <div class="toast-message" v-html="toasts.Accent.message"></div>
                </div>                
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
                Accent:     { title: '', message: '' },
                ErrMsg:     { title: '', message: '', stack: '' },
            },
        }
    },
    methods: {
        /** Displays a Colored Toast Notification at Bottom Right corner of the Window
         * @param data Configuration Object: 
          { 
               type: 'Info',            //<- Info, Success, Warning, Error, Accent
               title: '',               //<- [Optional] Title of the Toast
               message: '',             //<- Message of the Toast, accepts HTML tags
               stack: '',               //<- [Optional] Only if 'type=Error', Stack Trace for Errors
               autoHide: true,          //<- [Optional] Toast hides automatically after a delay
               delay: 3000,             //<- [Optional] Auto-hide delay in milliseconds, 1s=1000ms, 1m=60000ms
               width:'460px',           //<- [Optional] Custom width
               accent: 'warning',       //<- [Optional] Only if 'type=Accent', Color for the Accent bar. values: 'warning,success,error,info,accent'
               background: 'primary',   //<- [Optional] Only if 'type=Accent', Background color with contrasting foreground color. values: 'primary,secondary,light,dark,success,warning,danger,info'
          } */
        showToast(data) {
            const {
                type = 'Info',
                title = '',
                message = '',
                stack,
                autoHide = true,
                delay = 5000, //<- 5 seconds default
                width,
                accent = 'warning',      // solo para Accent
                background = 'info'   // solo para Accent
            } = data;

            const toastType = type.charAt(0).toUpperCase() + type.slice(1);

            if (this.toasts[toastType]) {
                this.toasts[toastType].title = title;
                this.toasts[toastType].message = message;
                if (stack) this.toasts[toastType].stack = stack;

                const toast = document.getElementById(`liveToast-${toastType}`);

                // Solo si es Accent aplicamos clases dinámicas
                if (toastType === 'Accent') {
                    // Limpiar clases previas
                    toast.classList.remove(
                        'accent-warning', 'accent-success', 'accent-error', 'accent-info', 'accent-accent',
                        'text-bg-primary', 'text-bg-success', 'text-bg-warning', 'text-bg-danger', 'text-bg-info'
                    );
                    if (accent) toast.classList.add(`accent-${accent}`);
                    if (background) toast.classList.add(`text-bg-${background}`);
                }

                // Apply width if provided
                toast.style.width = width || '';

                let toastBootstrap = bootstrap.Toast.getInstance(toast);
                const options = { autohide: autoHide, delay };

                if (toastBootstrap) toastBootstrap.dispose();
                toastBootstrap = new bootstrap.Toast(toast, options);
                toastBootstrap.show();
            } else {
                console.error(`Toast type "${type}" not recognized.`);
            }
        },
  /*      showToast(data) {
            const {
                type,
                title,
                message,
                stack,
                autoHide = true,
                delay = 3000,
                width // <- new
            } = data;
            const toastType = type.charAt(0).toUpperCase() + type.slice(1);

            try {
                if (this.toasts[toastType]) {
                    this.toasts[toastType].title = title;
                    this.toasts[toastType].message = message;
                    if (stack) this.toasts[toastType].stack = stack;
                    console.log('Toast:', this.toasts[toastType]);

                    //const toast = document.getElementById(`liveToast-${toastType}`);
                    const toast = document.getElementById(`liveToast-${toastType}`);

                    // Limpia clases previas de acento
                    toast.classList.remove(
                    'toast-accent-info',
                    'toast-accent-success',
                    'toast-accent-warning',
                    'toast-accent-error',
                    'toast-accent-accent'
                    );
                    // Aplica la clase según el tipo
                    toast.classList.add(`toast-accent-${toastType.toLowerCase()}`);

                    // Apply width if provided
                    if (width) {
                        toast.style.width = width;
                    } else {
                        toast.style.width = ''; // fallback to CSS/default
                    }

                    let toastBootstrap = bootstrap.Toast.getInstance(toast);
                    const options = { autohide: autoHide, delay };

                    if (toastBootstrap) {
                        toastBootstrap.dispose();
                    }
                    toastBootstrap = new bootstrap.Toast(toast, options);
                    toastBootstrap.show();

                    if (toastType === 'ErrMsg') {
                        // this probably meant to log 'data', not 'error'
                        if (message && stack) {
                            window.api.logError(message, stack);
                        }
                    }

                } else {
                    console.error(`Toast type "${type}" not recognized.`);
                }
            } catch (error) {
                console.log(error.message + error.stack);
            }
        },*/

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

.custom-toast {
  position: relative;
  padding-left: 1rem; /* espacio para que el texto no quede pegado */
  background-color: #fff8e1; /* ejemplo: amarillo claro */
  border-radius: 0.5rem;
}
/* Barra vertical */
.custom-toast::before {
  content: "";
  position: absolute;
  top: 0;
  left: 0;
  width: 12px; /* entre 10 y 15px según prefieras */
  height: 100%;
  background-color: #fbc02d; /* color de acento */
  border-top-left-radius: 0.5rem;
  border-bottom-left-radius: 0.5rem;
}
.custom-toast .toast-body {
  padding-right: 2rem;
}
.custom-toast .btn-close {
  position: absolute;
  top: 0.25rem;
  right: 0.25rem;
}
.custom-toast .d-flex {
  align-items: center;
}
.accent-warning::before { background-color: #fbc02d; }
.accent-success::before { background-color: #4caf50; }
.accent-error::before   { background-color: #f44336; }
.accent-info::before    { background-color: #2196f3; }
.accent-accent::before  { background-color: #9c27b0; }
</style>