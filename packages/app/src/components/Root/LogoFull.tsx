import React from 'react';
import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles({
  svg: {
    width: 'auto',
    height: 28,
  },
  path: {
    fill: '#7df3e1',
  },
});

const LogoFull = () => {
  const classes = useStyles();

  return (
    <svg
      className={classes.svg}
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 1000 300"
    >
      <text
        x="50"
        y="200"
        fontSize="180"
        fontFamily="Arial, sans-serif"
        fontWeight="bold"
        className={classes.path}
      >
        Backstage
      </text>
    </svg>
  );
};

export default LogoFull;
