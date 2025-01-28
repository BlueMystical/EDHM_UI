<template>
    <div v-if="progress > 0">
        <progress :value="progress" max="100">{{ progress }}%</progress>
    </div>
</template>

<script>
export default {
    data() {
        return {
            progress: 0,
        };
    },
    methods: {
        async StartDownload(pReleaseInfo) {

            console.log(pReleaseInfo.assets[0].url)
            const url = pReleaseInfo.assets[0].url;
            const destFolder = await window.api.resolveEnvVariables('%LOCALAPPDATA%\\Temp\\EDHM_UI');
            window.api.ensureDirectoryExists(destFolder);
            const destFilePath = window.api.joinPath(destFolder, 'EDHM-UI-V3.Setup.exe'); //TODO: different SOs have different installers
            console.log(destFilePath);

            window.api.downloadAsset(url, destFilePath)
                .then(() => {
                    console.log('Download complete');
                    this.$emit('onComplete', destFilePath);
                    // Proceed to run the installer or notify the user

                })
                .catch(error => {
                    console.error('Error downloading asset:', error);
                });

            window.api.onDownloadProgress((receivedBytes, totalBytes) => {
                this.progress = (receivedBytes / totalBytes) * 100;
            });
        }
    }
};
</script>