// Call the dataTables jQuery plugin
$(document).ready(function() {
  $('#dataTable').DataTable();
});
// $(document).ready(function() {
//   $('.modal-datatable').DataTable();
// });
$('.modal').on('shown.bs.modal', function (e) {
  $('.modal-datatable').DataTable();
});