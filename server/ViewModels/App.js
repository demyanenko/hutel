import React from 'react';
import injectTapEventPlugin from 'react-tap-event-plugin';
import { HashRouter, Route } from 'react-router-dom';

import $ from 'jquery';
import moment from 'moment';

import * as Colors from 'material-ui/styles/colors';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import getMuiTheme from 'material-ui/styles/getMuiTheme';
import Paper from 'material-ui/Paper';

import * as Constants from './Constants';
import EditRawData from './EditRawData';
import Home from './Home';
import Loading from './Loading';
import SubmitPoint from './SubmitPoint';

injectTapEventPlugin();

class App extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      tags: [],
      points: [],
      charts: [],
      chartsPoints: [],
      tagsLoading: true,
      pointsLoading: true,
      chartsLoading: true,
      chartsPointsLeftToLoad: 0
    };
  }

  componentDidMount() {
    this.updatePoints();
    this.updateTags();
    this.updateCharts();
  }

  updatePoints() {
    this.setState({pointsLoading: true}, () => {
      $.ajax({
        url: "/api/points",
        data: {
          startDate: moment().subtract(7, 'days').format(Constants.dateFormat)
        },
        dataType: 'json',
        cache: false,
        success: (data) => {
          this.setState({points: data, pointsLoading: false});
        },
        error: (xhr, status, err) => {
          console.error(err);
          this.setState({pointsLoading: false});
        }
      });
    });
  }

  updateTags() {
    this.setState({tagsLoading: true}, () => {
      $.ajax({
        url: "/api/tags",
        dataType: "json",
        cache: false,
        success: (data) => {
          this.setState({tags: data, tagsLoading: false});
        },
        error: (xhr, status, err) => {
          console.log(err);
          this.setState({tagsLoading: false});
        }
      });
    });
  }
  
  updateCharts() {
    this.setState({
      charts: [],
      chartsLoading: false,
      chartsPointsLeftToLoad: 0
    });
    
    /*
    this.setState({chartsLoading: true}, () => {
      $.ajax({
        url: "/api/charts",
        dataType: "json",
        cache: false,
        success: (data) => {
          this.setState({
            charts: data,
            chartsLoading: false,
            chartsPointsLeftToLoad: data.length
          }, this.updateChartsPoints);
        },
        error: (xhr, status, err) => {
          console.error(err);
          this.setState({chartsLoading: false});
        }
      });
    });
    */
  }

  updateChartsPoints() {
    this.state.charts.map((chart, index) => {
      $.ajax({
        url: "/api/points",
        data: {
          startDate: moment()
            .subtract(chart.time, chart.timeUnit)
            .format(Constants.dateFormat),
          tagId: chart.tagId
        },
        dataType: 'json',
        cache: false,
        success: (data) => {
          this.setState((prevState) => {
            var chartsPoints = prevState.chartsPoints.slice();
            chartsPoints[index] = data;
            return {
              chartsPoints: chartsPoints,
              chartsPointsLeftToLoad: prevState.chartsPointsLeftToLoad - 1
            };
          });
        },
        error: (xhr, status, err) => {
          console.error(err);
          this.setState((prevState) => ({
            chartsPointsLeftToLoad: prevState.chartsPointsLeftToLoad - 1
          }));
        }
      });
    });
  }

  onPointsChanged() {
    this.updatePoints();
    this.setState({
      chartsPointsLeftToLoad: this.state.charts.length
    }, this.updateChartsPoints);
  }

  render() {
    const theme = {
      headerBackground: Colors.deepPurple900,
      headerText: Colors.fullWhite,
      headerMuiTheme: getMuiTheme({
        palette: {
          primary1Color: Colors.deepPurple900
        }
      }),
      topBackground: Colors.fullWhite,
      historyBackground: Colors.grey100,
      historyDateText: Colors.deepPurple900
    };
    const muiTheme = getMuiTheme({
      palette: {
        primary1Color: Colors.amber900,
        accent1Color: Colors.yellow500
      }
    });
    const columnStyle = {
      maxWidth: 800,
      margin: "auto",
      display: "flex",
      flexDirection: "column",
      flexHeight: "100%",
      flexMinHeight: "100%"
    };
    const body = this.state.tagsLoading || this.state.pointsLoading || this.state.chartsLoading || this.state.chartsPointsLeftToLoad > 0
      ? <Loading theme={theme} />
      : <div>
          <div>
            <Route exact path="/" render={(props) => (
              <Home
                {...props}
                tags={this.state.tags}
                points={this.state.points}
                charts={this.state.charts}
                chartsPoints={this.state.chartsPoints}
                theme={theme}
              />
            )} />
          </div>
          <div>
            <Route path="/submit/:tag" render={(props) => (
              <SubmitPoint
                {...props}
                tag={this.state.tags.find(tag => tag.id === props.match.params.tag)}
                theme={theme}
                points={this.state.points}
                notifyPointsChanged={this.onPointsChanged.bind(this)}
              />
            )} />
          </div>
          <div>
            <Route exact path="/edit/tags" render={(props) => (
              <EditRawData
                {...props}
                theme={theme}
                url={"/api/tags"}
                title={"Edit Raw Tags"}
                floatingLabel={"Tags"}
              />
            )} />
          </div>
          <div>
            <Route exact path="/edit/points" render={(props) => (
              <EditRawData
                {...props}
                theme={theme}
                url={"/api/points"}
                title={"Edit Raw Points"}
                floatingLabel={"Points"}
              />
            )} />
          </div>
          <div>
            <Route exact path="/edit/charts" render={(props) => (
              <EditRawData
                {...props}
                theme={theme}
                url={"/api/charts"}
                title={"Edit Raw Charts"}
                floatingLabel={"Charts"}
              />
            )} />
          </div>
        </div>;
    return (
      <HashRouter>
        <MuiThemeProvider muiTheme={muiTheme}>
          <Paper
            style={columnStyle}
            zDepth={5}
          >
          {body}
          </Paper>
        </MuiThemeProvider>
      </HashRouter>
    );
  }
}

export default App;
