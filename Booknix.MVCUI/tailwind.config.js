module.exports = {
  content: [
    "./Views/**/*.cshtml", // Razor sayfalarındaki class'ları içerecek
    "./Pages/**/*.cshtml", // Eğer sayfalar varsa
    "./wwwroot/js/**/*.js", // JavaScript içinde kullanılan class'ları içerecek
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ["Poppins", "Arial", "sans-serif"], // Burada 'Poppins' fontunu ekledik
      },
    },
  },
  plugins: [],
};
