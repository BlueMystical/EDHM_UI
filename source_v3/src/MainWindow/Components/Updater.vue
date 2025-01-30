<template>
    <div v-if="progress > 0">
        <!--<progress :value="progress" max="100">{{ progress }}%</progress>-->
        <progress v-if="isDownloading" :value="downloadProgress" max="100"></progress>
    </div>
</template>

<script>
import EventBus from '../../EventBus.js';
export default {
    data() {
        return {
            isDownloading: false,
            downloadProgress: 0,
            downloadComplete: false,
            downloadError: null,
        };
    },
    methods: {
        async startDownload(pReleaseInfo) {

            this.isDownloading = true;
            
            console.log(pReleaseInfo);
            const url = pReleaseInfo.assets[0].url;
            const destFolder = await window.api.resolveEnvVariables('%LOCALAPPDATA%\\Temp\\EDHM_UI');
            await window.api.ensureDirectoryExists(destFolder);

            const filePath = window.api.joinPath(destFolder, 'EDHM-UI-V3.Setup.exe'); //TODO: different SOs have different installers
            console.log(filePath);

            window.api.openUrlInBrowser(url);
            
           /* window.api.downloadFile(url, filePath);
            window.api.onDownloadProgress((progress) => {
                console.log(`Download progress: ${progress}%`);
                // Optionally, update your UI with the progress
            });*/
        },

        OnDownload_Progress(progress) {
            console.log(progress);
            this.downloadProgress = progress;
        },
        OnDownload_Complete() {
            this.isDownloading = false;
            this.downloadComplete = true;
            console.log('Complete');
            this.$emit('onComplete', destFilePath);
        },
        OnDownload_Error(errorMessage) {
            this.isDownloading = false;
            this.downloadError = errorMessage;
            console.log(errorMessage);
        },

    },
    mounted() {
        /* EVENTS WE LISTEN TO HERE:  */
        EventBus.on('download-progress', this.OnDownload_Progress);
        EventBus.on('download-complete', this.OnDownload_Complete);
        EventBus.on('download-error', this.OnDownload_Error);
    },
    beforeUnmount() {
        // Clean up the event listener
        EventBus.off('download-progress', this.OnDownload_Progress);
        EventBus.off('download-complete', this.OnDownload_Complete);
        EventBus.off('download-error', this.OnDownload_Error);
    }
};
</script>