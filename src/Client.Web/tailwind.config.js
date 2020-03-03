module.exports = {
  theme: {
      extend: {
          colors: {
              'white': '#ffffff',
              'gray-lightest': '#f8f9f8',
              'gray-lighter': '#e4e5e4',
              'gray-light': '#d0d1d0',
              'gray': '#bebebd',
              'gray-dark': '#969796',
              'gray-darker': '#6e6f6e',
              'gray-darkest': '#484948',
              'black': '#202120',

              'brand-light': '#c9dec3',
              'brand': '#127b19',
              'brand-dark': '#173e14',

              'cta-light': '#d3c1dd',
              'cta': '#4d127b',
              'cta-dark': '#2a113d',

              'info-light': '#d7f2f3',
              'info': '#3ec9d0',
              'info-dark': '#295f62',

              'warning-light': '#f9f1ca',
              'warning': '#d4ca0c',
              'warning-dark': '#666018',

              'success-light': '#d6f6cf',
              'success': '#2cd63b',
              'success-dark': '#256624',

              'danger-light': '#fed5cb',
              'danger': '#d9563c',
              'danger-dark': '#692e21',

              'github-color': '#24292E'
          }
      },
      spacing: {
          px: '1px',
          '00': '0',
          '01': '0.25rem',
          '02': '0.5rem',
          '03': '0.75rem',
          '04': '1rem',
          '05': '1.25rem',
          '06': '1.5rem',
          '08': '2rem',
          '10': '2.5rem',
          '12': '3rem',
          '16': '4rem',
          '20': '5rem',
          '24': '6rem',
          '32': '8rem',
          '40': '10rem',
          '48': '12rem',
          '56': '14rem',
          '64': '16rem',
      },
      order: {
          first: '-9999',
          last: '9999',
          none: '0',
          '01': '1',
          '02': '2',
          '03': '3',
          '04': '4',
          '05': '5',
          '06': '6',
          '07': '7',
          '08': '8',
          '09': '9',
          '10': '10',
          '11': '11',
          '12': '12',
      },
      width: theme => ({
          auto: 'auto',
          ...theme('spacing'),
          '01/02': '50%',
          '01/03': '33.333333%',
          '02/03': '66.666667%',
          '01/04': '25%',
          '02/04': '50%',
          '03/04': '75%',
          '01/05': '20%',
          '02/05': '40%',
          '03/05': '60%',
          '04/05': '80%',
          '01/06': '16.666667%',
          '02/06': '33.333333%',
          '03/06': '50%',
          '04/06': '66.666667%',
          '05/06': '83.333333%',
          '01/12': '8.333333%',
          '02/12': '16.666667%',
          '03/12': '25%',
          '04/12': '33.333333%',
          '05/12': '41.666667%',
          '06/12': '50%',
          '07/12': '58.333333%',
          '08/12': '66.666667%',
          '09/12': '75%',
          '10/12': '83.333333%',
          '11/12': '91.666667%',
          full: '100%',
          screen: '100vw',
      }),
      screens: {
          'sm': { 'max': '767px' },
          'md': { 'min': '768px', 'max': '1023px' },
          'lg': { 'min': '1024px', 'max': '1279px' },
          'xl': { 'min': '1280px', 'max': '1919px' },
          '2xl': { 'min': '1920px' }
      },
  },
}
