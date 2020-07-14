﻿// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes


class DeathTracker {
    constructor(database, viewModel) {
        /// <summary>Responsible for tracking player deaths</summary>  
        this.deaths = [];
        this.minInterval = 8000;
        this.database = database;
        this.viewModel = viewModel;
    }

    process(events) {
        /// <summary>Processes events and checks for player deaths, if the player has died the entry is stored</summary>  
        /// <param name="events" type="Array">Events retrieved from Grim Dawn [{timestamp:0, event: 'Death'}]</param>  

        for (let i = 0; i < events.length; i++) {
            const event = events[i];

            //Only pop up if respawning. not for killed enemies
            if ( event.event === 'Respawning') { //event.event === 'Alive' || //if (event.event === 'Dead' || event.event === 'Dying') {
                // Minor cooldown on deaths, since the alert can come multiple times
                if (event.timestamp > this.lastDeath + this.minInterval) {
                    const entityId = this.database.getMainPlayerEntityId();
                    if (entityId) {
                        var death = {
                            timestamp: event.timestamp,
                            entityId: this.database.getMainPlayerEntityId()
                        };

                        this.deaths.push(death);
                        this.viewModel.add(death);
                    } else {
                        console.log('Death detected but no player found');
                    }
                } else {
                    console.log(
                        `Death ignored, timestamp ${event.timestamp} is too recent, expected >= ${this.lastDeath +
                        this.minInterval}`);
                }
            }
        }
    }

    get lastDeath() {
        /// <returns type="Number">The most recent timestamp of a player death.</returns>  
        return Math.max.apply(Math, this.deaths.map(function (o) { return o.timestamp; }));
    }

}
