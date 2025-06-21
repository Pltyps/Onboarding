module.exports = {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  safelist: [
    "bg-byuNavy",
    "dark:bg-byuRoyal",
    "bg-gray-100",
    "dark:bg-gray-700",
    "text-white",
    "text-black",
    "dark:text-white",
    "rounded-2xl",
    "rounded-br-sm",
    "rounded-bl-sm",
    "shadow",
  ],
  theme: {
    extend: {
      colors: {
        byuRoyal: "#003da5",
        byuLight: "#eeeef1",
        byuDark: "#202020",
        byuGray: "#a2adb1",
        byuNavy: "#002e5d",
      },
    },
  },
  darkMode: ["class", '[data-theme="dark"]'],
  plugins: [],
};
