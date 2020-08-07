import React from 'react';
import {
  Chart as BarChart,
  Series,
  CommonSeriesSettings,
  Font,
  Margin,
  Label,
  Format,
  Legend,
  ValueAxis,
  Title,
  Animation,
  ArgumentAxis,
  LoadingIndicator,
} from 'devextreme-react/chart';

import { useTranslation } from 'react-i18next';
import { Chart } from '../../models/common';
import { withTheme } from '@material-ui/core/styles';
import { Theme } from '@material-ui/core/styles/createMuiTheme';
interface Props {
  chart: Chart;
  theme: Theme;
}

const BarGraph = (props: Props) => {
  const { chart, theme } = props;
  const { t } = useTranslation();

  return (
    <BarChart
      palette="Material"
      paletteExtensionMode="blend"
      dataSource={JSON.parse(chart.dataSets)}
    >
      <CommonSeriesSettings
        argumentField="Label"
        type="bar"
        hoverMode="allArgumentPoints"
        selectionMode="allArgumentPoints"
      >
        <Label visible={true}>
          <Format type="fixedPoint" precision={0} />
        </Label>
      </CommonSeriesSettings>
      <Title text={t(chart.title)} horizontalAlignment="center">
        <Font size={30} color={theme.palette.primary.main} />
        <Margin top={25} />
      </Title>
      <Series valueField="Val0" name={t(chart.title)} />
      <ValueAxis position="right">
        <Title text="Count" />
      </ValueAxis>
      <ArgumentAxis>
        {' '}
        <Label
          displayMode="standard"
          rotationAngle={45}
          overlappingBehavior="rotate"
        />
      </ArgumentAxis>
      <Legend visible={false} />
      <LoadingIndicator enabled={true} />
      <Animation easing="linear" duration={500} maxPointCountSupported={100} />
    </BarChart>
  );
};

export default withTheme(BarGraph);

// class BarGraph extends React.Component<Props, {}> {
//   constructor(props) {
//     super(props);
//     console.log("CONSTRUCTOR");
//   }
//   componentDidMount() {
//     console.log("MOUNTED");
//   }

//   render() {
//     return (
//       <BarChart dataSource={JSON.parse(this.props.chart.dataSets)} title={this.props.chart.title}>
//         <CommonSeriesSettings
//           argumentField="Label"
//           type="bar"
//           hoverMode="allArgumentPoints"
//           selectionMode="allArgumentPoints"
//         >
//           <Label visible={true}>
//             <Format type="fixedPoint" precision={0} />
//           </Label>
//         </CommonSeriesSettings>
//         <Series
//           valueField="Val0"
//           name={this.props.chart.title} />
//         <ValueAxis position="right">
//           <Title text="Count" />
//         </ValueAxis>
//         <ArgumentAxis> {/* or ValueAxis, or CommonAxisSettings */}
//           <Label
//             displayMode="standard"
//             rotationAngle={45}
//             overlappingBehavior="rotate"
//           />
//         </ArgumentAxis>
//         <Legend visible={false}></Legend>
//         <LoadingIndicator enabled={true} />
//         <Animation
//           easing="linear"
//           duration={500}
//           maxPointCountSupported={100}
//         />
//       </BarChart>
//     );
//   }

//   onPointClick(e) {
//     e.target.select();
//   }
// }

// export default BarGraph
