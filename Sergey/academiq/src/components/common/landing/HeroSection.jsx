import React from 'react';
import { Grid2, Box, Typography, Paper } from '@mui/material';

const HeroSection = () => {
  return (
    <Paper
      elevation={3}
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        height: '100vh',
        padding: 4,
      }}
    >
      <Grid2
        container
        spacing={4} // Add spacing between columns
        sx={{
          maxWidth: 'lg', // Adjust max width as needed
          alignItems: 'center', // Align items vertically in the center
        }}
      >
        <Grid2 item xs={5} md={6} sx={{ display: 'flex', justifyContent: 'center' }}>
          <Box
            component="img"
            src="https://academiq-assets.s3.eu-north-1.amazonaws.com/academiq-logo.png"
            alt="logo"
            sx={{
              width: { xs: '300px', sm: '500px', md: '600px' },
              height: 'auto',
            }}
          />
        </Grid2>
        <Grid2 item xs={7} md={6} sx={{ display: 'flex', flexDirection: 'column', justifyContent: 'center' }}>
          <Typography
            variant="h1"
            sx={{
              textAlign: { xs: 'center', md: 'left' }, // Center on small screens, align left on larger
              padding: { xs: '0', sm: '0 16px' }, // Add padding on larger screens
              color: 'text.primary', // Use primary text color from theme
            }}
          >
            Welcome to AcademIQ
          </Typography>
          <Typography
            variant="h6"
            sx={{
              textAlign: { xs: 'center', md: 'left' }, // Center on small screens, align left on larger
              padding: { xs: '0', sm: '0 16px' }, // Add padding on larger screens
              color: 'text.secondary', // Use secondary text color from theme
              marginTop: 2,
            }}
          >
            An educational platform for schools and universities to manage students and courses in real-time.
          </Typography>
        </Grid2>
      </Grid2>
    </Paper>
  );
};

export default HeroSection;
