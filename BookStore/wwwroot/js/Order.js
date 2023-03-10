var dataTable;

$(document).ready(function () {
	const urlParams = new URLSearchParams(window.location.search);
	const param = urlParams.get('status');
	loadDataTable(param);
});

function loadDataTable(status) {
	dataTable = $('#tableData').DataTable({
		"ajax": {
			"url": "/Admin/Order/GetAll?status=" + status
		},
		"columns": [
			{ "data": "id", "width": "5%" },
			{ "data": "name", "width": "15%" },
			{ "data": "phoneNumber", "width": "15%" },
			{ "data": "applicationUser.email", "width": "15%" },
			{ "data": "orderStatus", "width": "15%" },
			{ "data": "orderTotal", "width": "10%" },
			{
				"data": "id",
				"render": function (data) {
					return `
					<div class="w-75 btn-group" role="group">
						<a href="/Admin/Order/Details?orderId=${data}" class="btn btn-primary mx-2" ><i class="bi bi-pencil-square"></i>Details</a>
					</div>
					`
				},
				"width" : "15%"
					
			}
		]
	});
}

