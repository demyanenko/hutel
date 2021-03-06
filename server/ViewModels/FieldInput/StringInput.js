import React from 'react';
import PropTypes from 'prop-types';

import TextField from 'material-ui/TextField';

class StringInput extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      value : this.props.field.defaultValue || "",
      validationMessage: ""
    };
  }

  onBlur(e) {
    var value = e.target.value;
    var success = !!value.length;
    var validationMessage = success ? "" : "value cannot be empty";
    this.setState({
      value: value,
      validationMessage: validationMessage
    });
    if (success) {
      this.props.onSuccessfulParse(value);
    }
  }

  onChange(e) {
    this.setState({
      value: e.target.value,
      validationMessage: ""
    });
  }

  render() {
    return (
      <div>
        <TextField
          name={this.props.field.name}
          floatingLabelText={this.props.field.name}
          floatingLabelFixed={true}
          value={this.state.value}
          errorText={this.state.validationMessage}
          onBlur={this.onBlur.bind(this)}
          onChange={this.onChange.bind(this)}
        ></TextField>
      </div>
    );
  }
}

StringInput.propTypes = {
  field: PropTypes.object,
  onSuccessfulParse: PropTypes.func,
  theme: PropTypes.object
};

export default StringInput;