import React, { useState, useEffect } from 'react'
import MomentUtils from '@date-io/moment';
import moment, { Moment } from 'moment';
import {
  MuiPickersUtilsProvider,
  KeyboardDatePicker,
} from '@material-ui/pickers';
import { useTranslation } from 'react-i18next';
import { Grid, makeStyles } from '@material-ui/core';
import { NullLogger } from '@aspnet/signalr';

const useStyles = makeStyles((theme) => ({
  small__fields: {
    width: 150,
    marginTop: 0,

  },
  "sub-text__padding": {
    marginTop: 5,
    fontStyle: 'italic',
  },
}));

interface Props {
  onValueChanged: (arg0: string) => void;
}

interface BetweenValue {
  left: Moment | null;
  right: Moment | null;
}


const FilterDateRangeField = (props: Props) => {
  const { onValueChanged } = props;
  const classes = useStyles();
  const { t } = useTranslation();
  const [betweenValue, setBetweenValue] = useState<BetweenValue>({
    left: null,
    right: null,
  });

  useEffect(() => {
    onValueChanged(`${betweenValue.left?.format()}|${betweenValue.right?.format()}`);
  }, [betweenValue, onValueChanged]);

  const leftChanged = (date: Moment | null) => {
    if (date !== null) {
      setBetweenValue((state) => ({ ...state, left: date }));
    }
  }

  const rightChanged = (date: Moment | null) => {
    if (date !== null) {
      setBetweenValue((state) => ({ ...state, right: date }));
    }
  }

  return (
    <Grid container direction="row" spacing={1}>
      <Grid item>
        <MuiPickersUtilsProvider utils={MomentUtils} locale={moment.locale()}>
          <KeyboardDatePicker
            margin="normal"
            format={moment().local().localeData().longDateFormat("L")}
            value={betweenValue.left}
            inputVariant="standard"
            autoOk
            onChange={leftChanged}
            className={classes.small__fields}
          />
        </MuiPickersUtilsProvider>
      </Grid>
      <Grid item>
        <div className={classes["sub-text__padding"]}>
          {t('COMMON.AND')}
        </div>
      </Grid>
      <Grid item>
        <MuiPickersUtilsProvider utils={MomentUtils} locale={moment.locale()}>
          <KeyboardDatePicker
            margin="normal"
            format={moment().local().localeData().longDateFormat("L")}
            value={betweenValue.right}
            inputVariant="standard"
            autoOk
            onChange={rightChanged}
            className={classes.small__fields}
          />
        </MuiPickersUtilsProvider>
      </Grid>
    </Grid>
  )
}

export default FilterDateRangeField
