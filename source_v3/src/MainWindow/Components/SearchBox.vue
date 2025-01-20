
<template>
    <div class="search-box-position">
        <div class="collapse collapse-horizontal" id="collapseWidthExample">
            <div class="card card-body search-box-card">
                <button type="button" class="btn-close" aria-label="Close" @click="hide" style="position: absolute; top: 10px; right: 10px;"></button>
                <div class="list-group search-box-list">
                    <template v-if="searchResults.length > 0">
                        <a v-for="(result, index) in searchResults" :key="index" href="#" class="list-group-item list-group-item-action" @click="handleClick(result)">
                            <div class="d-flex w-100 justify-content-between">
                                <h5 class="mb-1" style="color:gold;">{{ result.Title }}</h5>
                            </div>
                            <p class="mb-1" style="color: darkorange;">{{ result.Category }}</p>
                            <small>{{ result.Description }}</small>
                        </a>
                    </template>
                    <div v-else class="placeholder bg-secondary">
                        <p>No results found.</p>
                    </div>
                </div> <!-- list-group -->
            </div>  <!-- card-body -->
        </div> <!-- collapse -->
    </div> 
</template>


<script>
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
            const collapseElement = document.getElementById('collapseWidthExample');
            const bsCollapse = new bootstrap.Collapse(collapseElement, {
                toggle: false
            });
            bsCollapse.show();
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
.search-box-position {
    position: absolute;
    top: 68px;
    left: 6px;    
    width: 500px;
}
.search-box-card {
    height: calc(100vh - 130px);
    position: relative;
}
.search-box-list {
    width: 440px;
    height: 100%;
    overflow-y: auto;
}

.placeholder {
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
}

/* Custom scrollbar styles */
.search-box-list::-webkit-scrollbar {
    width: 6px;
}
.search-box-list::-webkit-scrollbar-track {
    background: #f1f1f1;
}
.search-box-list::-webkit-scrollbar-thumb {
    background: #888;
    border-radius: 3px;
}
.search-box-list::-webkit-scrollbar-thumb:hover {
    background: #555;
}
</style>
