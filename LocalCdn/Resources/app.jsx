var EntryRow = React.createClass({
    render: function() {
        return (<li className='list-group-item'>{this.props.name}</li>);
    }
});

var LocalCdn = React.createClass({
    getInitialState: function () {
        return {
            entries: []
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

    render: function() {
        var entries = [];

        this.state.entries.forEach(function(entry) {
            entries.push(<EntryRow name={entry.Name} />);
        });

        return (<ul className="list-group">{entries}</ul>);
    }
});

setInterval(function() {
    React.render(<LocalCdn />, document.getElementById('view'));
}, 500);