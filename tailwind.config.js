/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    "./**/*.{cshtml,razor,html,cs}",
    "./**/*.cs",
    "./wwwroot/index.html"
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}