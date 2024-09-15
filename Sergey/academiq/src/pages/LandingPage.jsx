import React from 'react';
import HeroSection from '../components/common/landing/HeroSection';
import SvgSection from '../components/common/landing/SvgSection';
import LandingButton from '../components/common/landing/LandingButton';

const LandingPage = () => {
    return (
        <div>
        <HeroSection />
        <SvgSection />
        <LandingButton />
        </div>
    )
};

export default LandingPage;