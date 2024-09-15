import { createTheme } from '@mui/material/styles';

const theme = createTheme({
  palette: {
    primary: {
      main: '#003366', // Primary1
      light: '#008080', // Primary2
      dark: '#FFFFFF', // Primary3
    },
    secondary: {
      main: '#40E0D0', // Secondary1
      light: '#FF7F50', // Secondary2
    },
    text: {
      primary: '#333333', // Text1
      secondary: '#708090', // Text2
    },
    background: {
      default: '#FFFFFF', // Background
      paper: '#FFFFFF', // Paper background, often used for cards
    },
    accent: {
      main: '#FFD700', // Accent color
    },
    action: {
      active: '#4169E1', // Button1
      hover: '#DC143C', // Button2
    },
    input: {
      bg: '#FFFFFF', // Input field background
      focus: '#F5F5F5', // Input field focus background
    },
  },
  typography: {
    fontFamily: 'Open Sans, sans-serif',
    h1: {
      fontSize: '40px',
      fontWeight: 700, // Bold
      lineHeight: '40px',
      letterSpacing: '0.07em',
    },
    h2: {
      fontSize: '32px',
      fontWeight: 400, // Regular
      lineHeight: '40px',
      letterSpacing: '0.07em',
    },
    h3: {
      fontSize: '24px',
      fontWeight: 400, // Regular
      lineHeight: '32px',
      letterSpacing: '0em',
    },
    h4: {
      fontSize: '20px',
      fontWeight: 400, // Regular
      lineHeight: '28px',
      letterSpacing: '0.07em',
    },
    body1: {
      fontSize: '16px',
      fontWeight: 400, // Regular
      lineHeight: '24px',
      letterSpacing: '0.07em',
    },
    cardTitle: {
      fontSize: '18px',
      fontWeight: 400, // Regular
      lineHeight: '24px',
      letterSpacing: '0em',
    },
    cardBody: {
      fontSize: '14px',
      fontWeight: 400, // Regular
      lineHeight: '20px',
      letterSpacing: '0em',
    },
    sectionHeader: {
      fontSize: '18px',
      fontWeight: 700, // Bold
      lineHeight: '24px',
      letterSpacing: '0em',
    },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          color: '#FFFFFF',
          backgroundColor: '#4169E1', // Button1
          '&:hover': {
            backgroundColor: '#DC143C', // Button2
          },
        },
      },
    },
    MuiInputBase: {
      styleOverrides: {
        root: {
          backgroundColor: '#FFFFFF', // Input field background
          '&.Mui-focused': {
            backgroundColor: '#F5F5F5', // Input field focus background
          },
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundColor: '#F5F5F5', // Card background color
        },
      },
    },
  },
});

export default theme;
