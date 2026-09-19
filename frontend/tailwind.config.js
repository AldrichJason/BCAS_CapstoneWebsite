/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: {
          light: '#1f5c3f',
          DEFAULT: '#14432d',
          dark: '#0d2e1e',
          tint: '#e7f0ea',
        },
        accent: {
          tint: '#fbf1d3',
          DEFAULT: '#c9a227',
          dark: '#a3841f',
        },
      },
    },
  },
  plugins: [],
};
