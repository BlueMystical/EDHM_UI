<template>
    <div class="tab-content">
      <div class="scrollable-content">
        <table class="table table-bordered table-dark">
          <tbody>
            <tr v-for="n in 15" :key="'settings' + n">
              <td>Row {{ n }} Column 1</td>
              <td>
                <select class="form-control">
                  <option>Option 1</option>
                  <option>Option 2</option>
                  <option>Option 3</option>
                </select>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    name: 'UserSettingsTab'
  }
  </script>
  
  <style scoped>
  .tab-content {
    flex-grow: 1;
    display: flex;
    flex-direction: column;
    overflow-y: hidden;
  }
  
  .scrollable-content {
    flex-grow: 1;
    overflow-y: auto;
  }
  
  .table {
    margin: 10px 0;
    color: #f8f9fa;
    width: 100%;
  }
  </style>
  