// Call the dataTables jQuery plugin
$(document).ready(function () {
    $('#dataTable').DataTable({
        "order": [[5, "desc"]]
    });
});
// $(document).ready(function() {
//   $('.modal-datatable').DataTable();
// });
$('.modal').on('shown.bs.modal', function (e) {
    console.log(e);
    var id = "#"+e.currentTarget.id + " .modal-datatable";
    console.log(id);
    
    if (!$.fn.DataTable.isDataTable(id)) {
        $(id).DataTable({
            "order": [[0, "desc"]]
        });
    }
});