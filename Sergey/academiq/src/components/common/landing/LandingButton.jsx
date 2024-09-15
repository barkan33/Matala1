
import React from 'react';
import { Button, Box } from '@mui/material';
import { useNavigate } from 'react-router-dom';

const LandingButton = () => {
  const navigate = useNavigate();

  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
      <Button
        variant="contained"
        color="secondary"
        onClick={() => navigate('/login')}
      >
        Start Using AcademIQ
      </Button>
    </Box>
  );
};

export default LandingButton;
