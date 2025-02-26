document.addEventListener("DOMContentLoaded", function () {
    // Ambil elemen h2 dengan class iTyped
    var dynamicTitle = document.querySelector('.iTyped');

    // Ambil data dari atribut data-typing-texts
    var typingTexts = JSON.parse(dynamicTitle.getAttribute('data-typing-texts'));

    // Inisialisasi iTyped dengan data dari database
    window.ityped.init(dynamicTitle, {
        strings: typingTexts,
        loop: true,
        typeSpeed: 100,
        backSpeed: 50,
        backDelay: 2000,
        disableBackTyping: true
    });
});
