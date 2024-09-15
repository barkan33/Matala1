import React from 'react';
import { Box, Grid2, Typography } from '@mui/material';
import svgSectionData from './svgSectionData.json'; 

const SvgSection = () => {
  const handleScroll = (id) => {
    document.getElementById(id).scrollIntoView({ behavior: 'smooth' });
  };

  return (
    <Box sx={{ p: 4 }}>
      <Grid2 container spacing={4}>
        {svgSectionData.map((section) => (
          <Grid2
            key={section.id}
            item
            xs={12}
            sm={4}
            onClick={() => handleScroll(section.id)}
          >
            <Box
              sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                cursor: 'pointer',
              }}
            >
              <img
                className="landing-svg"
                src={section.imgSrc}
                alt={section.altText}
              />
              <Typography variant="h6" align="center">
                {section.title}
              </Typography>
              <Typography align="center">{section.description}</Typography>
            </Box>
          </Grid2>
        ))}
      </Grid2>
    </Box>
  );
};

export default SvgSection;
