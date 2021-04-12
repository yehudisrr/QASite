$(() => {
    setInterval(() => {
        const id = $("#current-id").val();
        console.log(id);
        $.get('/home/getLikes', { id }, function (likes) {
            $("#likes-count").text(likes);
        });
    }, 500);

   
        $("#like-question").on('click', function () {
            const id = $(this).data("question-id");
            $.post('/home/like', { id }, function (likes) {
            })
            $(this).removeClass('bi bi-suit-heart').addClass('bi bi-suit-heart-fill');
      
    });
});
      
