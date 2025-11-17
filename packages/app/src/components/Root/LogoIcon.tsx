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

const LogoIcon = () => {
  const classes = useStyles();

  return (
    <svg
      className={classes.svg}
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 100 100"
    >
      <circle cx="50" cy="50" r="40" className={classes.path} />
      <text
        x="50"
        y="65"
        fontSize="40"
        fontFamily="Arial, sans-serif"
        fontWeight="bold"
        textAnchor="middle"
        fill="#000"
      >
        B
      </text>
    </svg>
  );
};

export default LogoIcon;
