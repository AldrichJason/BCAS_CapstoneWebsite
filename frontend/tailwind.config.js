/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: {
          light: '#1d744b',
          DEFAULT: '#0f3b26',
          dark: '#071a11',
          tint: '#eaf5f0',
        },
        accent: {
          tint: '#e1d6b0',
          DEFAULT: '#d4af37',
          dark: '#a18323',
        },
      },
    },
  },
  plugins: [],
};
