function themdanhmuc() {
    let data = {};
    let formdata = $('#add-form').serializeArray({

    });
    $.each(formdata, function (index, value) {
        data["" + value.name + ""] = value.value;

    });
    $.ajax({
        url: '/Danhmucs/Create',
        type: 'post',
        contentType: 'application/json',
        data: JSON.stringify(data),
        dataType: 'json',
        success: function (repone) {
            if (repone.status == true) {
                $("#add-mess").addClass("text-warning");
                setTimeout(function () {

                    window.location.replace("/Admin/Danhmucs");


                }, 1000)

            }
            else {

                $("#add-mess").addClass("text-danger");

            }
            $("#add-mess").html(repone.message);

        },
        error: function (repone) {

            console.log(repone);

        }
    });


    return false;
}
function loaddata(id) {
    $("#update-mess")
        .removeClass("text-warning text-danger")
        .html("");
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/Danhmucs/Loaddata',
        success: function (respone) {
            $("#madm").val(respone.MaDM);
            $("#tendm").val(respone.TenDM);

        },
        error: function (respone) {
            console.log(xhr.respone);
            alert("errorr...");


        }


    });

}
function suadanhmuc() {
    let data = {};
    let formdata = $('#update-form').serializeArray({});
    $.each(formdata, function (index, value) {
        data["" + value.name + ""] = value.value;


    });
    $.ajax({
        url: '/Danhmucs/Update',
        type: 'post',
        data: JSON.stringify(data),
        datatype: 'json',
        contentType:'application/json',
        success: function (respone) {
            $("#update-mess").html(respone.message);
            if (respone.status == true) {
                $("#update-mess").addClass("text-warning");
                setTimeout(function () {

                    window.location.replace("/Admin/Danhmucs")
                }, 1000)
            }
            else {
                $("#update-mess").addClass("text-danger");
               


                }
        },
        error: function (respone) { 
        console.log(respone)

        }
    });
    return false;

}
function deleteData(id) {
    $("#delete-madm").val(id);
}
function xoadanhmuc() {
    let id = $("#delete-madm").val();
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/Danhmucs/Delete',
        success: function (respone) {
            if (respone.status == true) {
                $(".cancelPopup").click();
                $("#row-" + id).remove();
                setTimeout(function () {
                    window.location.replace('/Admin/Danhmucs')

                }, 300);
            }
        },
        error: function (respone) {
            console.log(xhr.respone);
            alert("errorr...");


        }
    });

}















