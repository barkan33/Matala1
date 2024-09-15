import React from 'react';
import { useParams } from 'react-router-dom';

const CoursePage = () => {
  const { id } = useParams();
  return <h1>Course Page for {id}</h1>;
};

export default CoursePage;
