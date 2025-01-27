
<template>

    <div id="containerSearchBox" class="offcanvas offcanvas-start" data-bs-scroll="true" tabindex="-1" aria-labelledby="offcanvasWithBothOptionsLabel">
        <div class="offcanvas-header">
            <h5 class="offcanvas-title" id="offcanvasWithBothOptionsLabel">Search Results:</h5>
            <button type="button" class="btn-close" data-bs-dismiss="offcanvas" aria-label="Close"></button>
        </div>
        <div class="offcanvas-body">

            <div class="list-group ">
                <template v-if="searchResults.length > 0">
                    <a v-for="(result, index) in searchResults" :key="index" href="#" class="list-group-item list-group-item-action" @click="handleClick(result)">
                        <div class="d-flex w-100 justify-content-between">
                            <h5 class="mb-1" style="color:gold;" v-html="result.Title"></h5>
                        </div>
                        <p class="mb-1" style="color: darkorange;" v-html="result.Category"></p>
                        <small v-html="result.Description"></small>
                    </a>
                </template>
                <div v-else class="placeholder bg-secondary text-light h-auto w-auto p-3">
                    <p>No results found.</p>
                </div>
            </div> <!-- list-group -->

        </div>
    </div>

</template>
<script>
// Enable the OffCanvas:
const offcanvasElementList = document.querySelectorAll('.offcanvas');
const offcanvasList = [...offcanvasElementList].map(offcanvasEl => new bootstrap.Offcanvas(offcanvasEl));

export default {
    name: 'SearchBox',
    props: {
        searchResults: {
            type: Array,
            required: true
        }
    },
    methods: {
        handleClick(result) {
            this.$emit('resultClicked', result);
        },
        show() {
            const myOffcanvas = document.getElementById('containerSearchBox');
            const bsOffcanvas = new bootstrap.Offcanvas(myOffcanvas, { scroll: true, backdrop: true });
            myOffcanvas.addEventListener('hidden.bs.offcanvas', event => {
            // do something...
            });
            bsOffcanvas.show();
        },
        hide() {
            const collapseElement = document.getElementById('collapseWidthExample');
            const bsCollapse = new bootstrap.Collapse(collapseElement, {
                toggle: false
            });
            bsCollapse.hide();
        }
    }
}
</script>

<style scoped>
</style>
