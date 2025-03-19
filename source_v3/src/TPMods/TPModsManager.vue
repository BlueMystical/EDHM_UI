<template>
    <div id="TPModsManager" class="bg-dark text-light" data-bs-theme="dark">
        <h1>3rd Party Mods Manager</h1>
        <p>Manage your 3rd party mods here.</p>

    </div>
</template>

<script>
import EventBus from '../EventBus.js';
import Notifications from '../MainWindow/Components/Notifications.vue';
import Util from '../Helpers/Utils.js';

console.log('TPModsManager.vue loaded');


export default {
    name: 'TPModsManager',
    components: {
        //Notifications,
    },
    data() {
        return {
            loading: true,        //<- Flag to Show/hide the Loading Spinner
        };
    },
    methods: {

        /** This is the Start Point of the Program **/
        async Initialize() {
            try {
                console.log('Initializing 3PMods Manager..');


            } catch (error) {
                console.error(error);
            } finally {
                this.loading = false;
            }
        },

    },
    mounted() {
        this.Initialize();
        /* LISTENING EVENTS:   */
        EventBus.on('On_Initialize3PMods', this.Initialize);
    },
    beforeUnmount() {
        // Clean up the event listener
        EventBus.off('On_Initialize3PMods', this.Initialize);
    }
};
</script>

<style scoped>
#TPModsManager {
    background-color: #1F1F1F;
    color: #ffffff;
}
.visually-hidden {
    color: #ffffff;
}
</style>
