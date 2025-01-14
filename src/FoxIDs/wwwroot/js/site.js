﻿(function () {
    var setFocus = true;

    $('.input-control').each(function () {
        var input = $(this);
        var parent = $(this.parentNode);

        if (setFocus) {
            input.focus();
            setFocus = false;
        }

        if (input.val()) {
            parent.addClass('active');
        }

        input.blur(function () {
            if (input.val()) {
                parent.addClass('active');
            }
            else {
                parent.removeClass('active');
            }
        });
    });

    function browserValueCheck() {
        $('.input-control:not(.active)').each(function () {
            if ($(this).val()) {
                $(this.parentNode).addClass('active');
            }
        });
    }
    setInterval(browserValueCheck, 100);

    function setDisableOnSubmit() {
        $("form").submit(function (event) {
            if (!this.submitDisable) {
                if ($(this).valid()) {
                    $("input[type=submit]", this).attr("disabled", true);
                    this.submitDisable = true;
                }
            }
            else {
                event.preventDefault();
            }
        });
    }
    setDisableOnSubmit();

})();


