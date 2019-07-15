<template>
    <v-app>
        <v-navigation-drawer
                fixed
                app
        >
            <v-divider></v-divider>
            <v-list>
                <v-list-tile>
                    <v-list-tile-content>
                        <v-btn block depressed color="indigo lighten-4" @click="open">Open Genetic Algorithm Tasks</v-btn>
                    </v-list-tile-content>
                </v-list-tile>
            </v-list>


            <v-toolbar flat>
                <v-list>
                    <v-list-tile>
                        <v-list-tile-title class="title">
                            Data Group
                        </v-list-tile-title>

                        <v-btn depressed small outline color="indigo">Add Data</v-btn>
                    </v-list-tile>
                </v-list>
            </v-toolbar>

            <v-divider></v-divider>
            <v-list>
                <v-list-tile
                        v-for="group in groups"
                        :key="group.id"
                        :to="`/${group.id}`"
                >
                    <v-list-tile-content>
                        <v-list-tile-title>{{ group.name }}</v-list-tile-title>
                    </v-list-tile-content>
                </v-list-tile>
            </v-list>
        </v-navigation-drawer>

        <v-dialog v-model="dialog" persistent fullscreen hide-overlay transition="dialog-bottom-transition">
            <v-card>
                <v-navigation-drawer
                        stateless
                        absolute
                        :value="dialog"
                >
                    <v-list>
                        <v-list-tile>
                            <v-list-tile-title class="title">
                                Genetic Algorithm Tasks
                            </v-list-tile-title>
                        </v-list-tile>
                        <v-list-tile>
                            <v-btn depressed small outline color="grey" @click="close">Close</v-btn>
                            <v-btn depressed small outline color="blue" @click="createNewTask"
                                   :disabled="states.creatingTask">New Task
                            </v-btn>
                        </v-list-tile>
                        <v-list-tile avatar v-for="task in tasks" :key="task.id" @click="selectedTask = task">
                            <v-list-tile-content>
                                <v-list-tile-title>{{ task.id }}</v-list-tile-title>
                                <v-list-tile-sub-title>{{ task.group.name }}</v-list-tile-sub-title>
                                <v-list-tile-sub-title>{{ task.state }}</v-list-tile-sub-title>
                            </v-list-tile-content>
                        </v-list-tile>
                    </v-list>
                </v-navigation-drawer>
                <v-content>
                    <v-layout justify-center v-if="states.creatingTask">
                        <v-flex xs12 sm10 md5 lg3>
                            <v-card flat>
                                <v-card-text>
                                    Data
                                    <v-select :items="groups.map(group => ({value: group.id, text: group.name}))"
                                              label="Data Group"
                                              v-model="newTask.group"
                                    ></v-select>

                                    Coefficients
                                    <v-text-field v-for="coefficient in newTask.coefficients"
                                                  :key="coefficient.key"
                                                  :label="coefficient.key"
                                                  v-model.number="coefficient.value"
                                                  type="number"
                                    ></v-text-field>

                                    Capacity
                                    <v-layout>
                                        <v-flex mx-2>
                                            <v-text-field label="Min"
                                                          v-model.number="newTask.capacity.min"
                                                          type="number"
                                            ></v-text-field>
                                        </v-flex>
                                        <v-flex mx-2>
                                            <v-text-field label="Max"
                                                          v-model.number="newTask.capacity.max"
                                                          type="number"
                                            ></v-text-field>
                                        </v-flex>
                                    </v-layout>
                                    
                                    <v-btn block outline color="blue" @click="registerTask">Register and Build</v-btn>
                                    <v-btn block outline color="orange" @click="cancelCreate">Cancel</v-btn>
                                </v-card-text>
                            </v-card>
                        </v-flex>
                    </v-layout>
                    <v-layout v-else-if="selectedTask">

                    </v-layout>
                </v-content>
            </v-card>
        </v-dialog>
        <v-content>
            <router-view></router-view>
        </v-content>
    </v-app>
</template>

<script>
    import HelloWorld from './components/HelloWorld'
    import axios from 'axios'
    import * as signalR from '@aspnet/signalr'

    const notification = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5000/notification')
        .configureLogging(signalR.LogLevel.Information)
        .build();

    export default {
        name: 'App',
        components: {
            HelloWorld
        },
        data() {
            return {
                groups: [],
                dialog: false,
                tasks: [],
                states: {
                    creatingTask: false
                },
                newTask: null,
                notification,
                selectedTask: null
            }
        },
        mounted() {
            axios.get('http://localhost:5000/api/data/group')
                .then(result => {
                    this.groups = result.data
                });

            axios.get('http://localhost:5000/api/ga')
                .then(result => {
                    this.tasks = result.data
                });

            this.notification.start().then(function () {
                console.log("notification connected");
            });
            
            this.notification.on("Building", taskId => {
                const task = this.tasks.find(task => task.id === taskId);
                task.state = "Building";
            });
            
            this.notification.on("BuildCompleted", taskId => {
                const task = this.tasks.find(task => task.id === taskId);
                task.state = "BuildCompleted";
            });
            
            this.notification.on("BuildFailed", taskId => {
                const task = this.tasks.find(task => task.id === taskId);
                task.state = "BuildFailed";
            })
        },
        methods: {
            open() {
                this.dialog = true
            },

            dialogNav() {
                return true;
            },

            close() {
                this.dialog = false
            },

            createNewTask() {
                axios.get('http://localhost:5000/api/data/coefficient')
                    .then(result => {
                        let coefficients = result.data.map(c => ({key: c, value: 0}));
                        this.newTask = {
                            group: 0,
                            coefficients,
                            capacity: {
                                max: 0,
                                min: 0
                            }
                        };
                        this.states.creatingTask = true;
                    })
            },
            
            cancelCreate() {
                this.newTask = null;
                this.states.creatingTask = false;
            },

            registerTask() {
                let coefficients = {};
                this.newTask.coefficients.forEach(c => {
                    coefficients[c.key] = c.value
                });
                axios.post("http://localhost:5000/api/ga", {
                    group: this.newTask.group,
                    coefficients,
                    max: this.newTask.capacity.max,
                    min: this.newTask.capacity.min,
                }).then(result => {
                    this.selectedTask = result.data;
                    this.tasks.push(result.data);
                    this.newTask = null;
                    this.states.creatingTask = false;
                    axios.put(`http://localhost:5000/api/ga/${this.selectedTask.id}`)
                        .then(result => console.log(result.data))
                })
            }
        }
    }
</script>
