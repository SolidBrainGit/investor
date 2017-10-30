﻿let userPostTags = [];

$(document).ready(function () {

    $("#logoff").click(function(e) {
        e.preventDefault();
        logOff();
    });

    $("#createUserPostForm").submit(function(e) {
        e.preventDefault();
        createPostAJAX(getCreatePostFormData());
    });

    initUserTinyMCE();
    getAllTags();

});


let getCreatePostFormData = function() {
    const formData = new FormData(document.getElementById("createUserPostForm"));

    let tagsArray = $("#userPostTags").tagsinput('items');
    console.log(tinyMCE.get('createUserPost').getContent());

    formData.append('Article', tinyMCE.get('createUserPost').getContent());    
    for (let i = 0; i < tagsArray.length; i++)
        formData.append(`Tags[` + i + `].Name`, tagsArray[i]);

    return formData;

}

let getAllTags = function () {
    $.ajax({
        url: "/api/Content/GetAllTags",
        type: "GET",
        success: function (data) {
            data.data.map(item => {
                userPostTags.push(item.name);
            });
            console.log(userPostTags);
            initUserTagsInput();
        }
    });
}

let substringMatcher = function (strs) {
    return function(q, cb) {
        let matches = [];
        let substringRegex = new RegExp(q, 'i');
        $.each(strs, function (i, str) {
            if (substringRegex.test(str)) {
                matches.push(str);
            }
        });
        cb(matches);
    };
};

let initUserTinyMCE = function () {

    tinymce.init({
        selector: "textarea#createUserPost",
        theme: "modern",
        width: 680,
        height: 300,
        plugins: [
            "advlist autolink link image lists charmap print preview hr anchor pagebreak spellchecker",
            "searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking",
            "save table contextmenu directionality emoticons template paste textcolor"
        ],
        content_css: "css/content.css",
        toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | print preview media fullpage | forecolor backcolor emoticons",
        style_formats: [
            { title: 'Bold text', inline: 'b' },
            { title: 'Red text', inline: 'span', styles: { color: '#ff0000' } },
            { title: 'Red header', block: 'h1', styles: { color: '#ff0000' } },
            { title: 'Example 1', inline: 'span', classes: 'example1' },
            { title: 'Example 2', inline: 'span', classes: 'example2' },
            { title: 'Table styles' },
            { title: 'Table row 1', selector: 'tr', classes: 'tablerow1' }
        ]
    }); 

}

let initUserTagsInput = function () {
    $("#userPostTags").tagsinput({
        typeaheadjs: {
            name: 'userPostTags',
            source: substringMatcher(userPostTags)
        }
    });
}

let logOff = function() {
    $.ajax({
        url: "/account/logoff",
        type: "POST",
        success: function (data) {
            window.location.href = data;
        }
    });
}

let createPostAJAX = function(fData) {
    $.ajax({
        url: "/account/createpost",
        type: "POST",
        data: fData,
        cache: false,
        contentType: false,
        processData: false,
        success: function (data) {
            window.location.href = data;
        }
    });
}