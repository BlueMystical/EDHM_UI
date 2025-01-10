<template>
    <div v-if="loading" class="d-flex justify-content-center align-items-center bg-dark text-light"
        style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; z-index: 9999;">
        <div class="bg-dark text-light" role="status">
            <span class="visually-hidden">Loading...</span>
        </div>
    </div>
</template>
<script>
import EventBus from '../../EventBus';
export default {
    name: 'WaitSpinner',
    components: {},
    data() {
        return {
            showSpinner: true,
        };
    },
    methods: {
        showHideSpinner(status) {
            this.showSpinner = status.visible;
            //EXAMPLE: ->    EventBus.emit('ShowSpinner', { visible: true } );//<- this event will be heard in 'MainNavBars.vue'
        },
    },
    mounted() {
        EventBus.on('ShowSpinner', this.showHideSpinner);
    },
    beforeUnmount() {
        EventBus.off('ShowSpinner', this.showHideSpinner);
    }
}
</script>
<style scoped></style>