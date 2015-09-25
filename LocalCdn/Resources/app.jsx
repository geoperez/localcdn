var EntryRow = React.createClass({
    render: function() {
        return (<li className='list-group-item'>{this.props.name}</li>);
    }
});

var List = React.createClass({
    getInitialState: function () {
        return {
            entries: []
        };
    },

    componentDidMount: function () {
        setInterval(function () {
            $.get(this.props.url, function (result) {
                if (this.isMounted()) {
                    this.setState({
                        entries: result
                    });
                }
            }.bind(this));
        }.bind(this), 5000);
    },

    render: function () {
        var entries = [];

        this.state.entries.forEach(function (entry) {
            entries.push(<EntryRow name={entry} />);
        });

        return (<div>{ entries }</div>);
    }
});

var Resources = React.createClass({
    getInitialState: function () {
        return {
            url: ''
        };
    },

    componentDidMount: function() {
        $.get("api/entries/", function(result) {
            if (this.isMounted()) {
                this.setState({
                    entries: result
                });
            }
        }.bind(this));
    },

    handleChange: function (event) {
        this.setState({ url: event.target.value });
    },

    handleClick: function () {
        $.ajax({
            type: 'POST',
            url: "api/entries",
            data: JSON.stringify({ name: '', url: this.state.url }),
            success: function (data) {
                console.log(data);
                this.setState({ url: '' });
            },
            contentType: "application/json",
            dataType: 'json'
        });
    },

    render: function() {
        return (
        <ul className="list-group">
            <li className="list-group-item">
                <div className="row">
                    <div className="col-xs-8">
                        <input type="text" value={this.state.url} onChange={this.handleChange} className="form-control" />
                    </div>
                    <div className="col-xs-4">
                        <button className="btn btn-default" onClick={this.handleClick}>Add</button>
                    </div>
                </div>
            </li>
            <List url={this.props.url} />
        </ul>);
    }
});

var Hosts = React.createClass({
    render: function () {
        return (<ul className="list-group"><List url={this.props.url} /></ul>);
    }
});


setInterval(function() {
    React.render(<Resources url="api/entries/" />, document.getElementById('resourcesView'));
    React.render(<Hosts url="api/hosts/" />, document.getElementById('hostView'));
}, 500);