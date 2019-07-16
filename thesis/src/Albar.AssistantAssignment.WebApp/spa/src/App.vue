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
                        <v-btn block depressed color="grey lighten-2" @click="open">Open Genetic Algorithm Tasks
                        </v-btn>
                    </v-list-tile-content>
                </v-list-tile>
            </v-list>


            <v-toolbar flat>
                <v-list>
                    <v-list-tile>
                        <v-list-tile-title class="title">
                            Data Group
                        </v-list-tile-title>

                        <v-btn depressed small outline color="indigo" @click="generateData">Generate Data</v-btn>
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
                        <v-list-tile-title>{{ group.name || group.id }}</v-list-tile-title>
                    </v-list-tile-content>
                </v-list-tile>
            </v-list>
        </v-navigation-drawer>

        <v-dialog v-model="dialog" fullscreen hide-overlay transition="dialog-bottom-transition">
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
                        <v-list-tile py-3 avatar v-for="task in tasks" :key="task.id" @click="selectedTask = task">
                            <v-list-tile-content>
                                <v-list-tile-title>{{ task.id }}</v-list-tile-title>
                                <v-list-tile-sub-title>{{ task.state }}</v-list-tile-sub-title>
                            </v-list-tile-content>
                        </v-list-tile>
                        <v-list-tile style="margin-top: 50px">
                            <v-list-tile-content>
                                <v-btn depressed small outline block color="blue" @click="createNewTask"
                                       :disabled="states.creatingTask">New Task
                                </v-btn>
                            </v-list-tile-content>
                        </v-list-tile>
                        <v-list-tile>
                            <v-list-tile-content>
                                <v-btn depressed small outline block color="grey" @click="dialog = false">
                                    Close
                                </v-btn>
                            </v-list-tile-content>
                        </v-list-tile>
                    </v-list>
                </v-navigation-drawer>
                <v-content style="padding-right: 0px">
                    <v-layout justify-center v-if="states.creatingTask">
                        <v-flex xs12 sm10 md5 lg3>
                            <v-card flat>
                                <v-card-title primary-title>
                                    Create Task
                                </v-card-title>
                                <v-card-text>
                                    Data
                                    <v-select :items="groups.map(group => ({value: group.id, text: group.name || group.id}))"
                                              label="Data Group"
                                              v-model="newTask.group"
                                    ></v-select>

                                    Objective Coefficients
                                    <v-text-field v-for="coefficient in newTask.coefficients"
                                                  :key="coefficient.key"
                                                  :label="coefficient.key"
                                                  v-model.number="coefficient.value"
                                                  type="number"
                                    ></v-text-field>

                                    Population Capacity
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

                                    <v-btn block outline color="blue" @click="registerTask">Add and Build</v-btn>
                                    <v-btn block outline color="orange" @click="cancelCreate">Cancel</v-btn>
                                </v-card-text>
                            </v-card>
                        </v-flex>
                    </v-layout>
                    <v-card v-else-if="selectedTask" style="margin: auto;" flat>
                        <v-toolbar flat>
                            <v-toolbar-title>TaskId: {{ selectedTask.id }}</v-toolbar-title>
                            <v-spacer></v-spacer>
                            <v-toolbar-title style="color:green">{{ selectedTask.state }}</v-toolbar-title>
                        </v-toolbar>
                        <v-container fluid pa-0>
                            <v-layout justify-space-between row>
                                <v-flex style="width: 100px !important;">
                                    <v-card flat>
                                        <v-list dense>
                                            <v-list-tile>
                                                <v-list-tile-content>
                                                    <v-list-tile-title><h3>Detail Task</h3></v-list-tile-title>
                                                </v-list-tile-content>
                                            </v-list-tile>
                                            <v-list-tile>
                                                <v-list-tile-content>Data Group</v-list-tile-content>
                                                <v-list-tile-content class="align-end">{{ selectedTask.group.name || selectedTask.group.id }}
                                                </v-list-tile-content>
                                            </v-list-tile>
                                            <v-list-tile>
                                                <v-list-tile-content>Subject Count</v-list-tile-content>
                                                <v-list-tile-content class="align-end">
                                                    {{ selectedTask.subjectCount }}
                                                </v-list-tile-content>
                                            </v-list-tile>
                                            <v-list-tile>
                                                <v-list-tile-content>Schedule Count</v-list-tile-content>
                                                <v-list-tile-content class="align-end">
                                                    {{ selectedTask.scheduleCount }}
                                                </v-list-tile-content>
                                            </v-list-tile>
                                            <v-list-tile>
                                                <v-list-tile-content>Assistant Count</v-list-tile-content>
                                                <v-list-tile-content class="align-end">
                                                    {{ selectedTask.assistantCount }}
                                                </v-list-tile-content>
                                            </v-list-tile>
                                            <v-divider></v-divider>
                                            <v-list-tile>
                                                <v-list-tile-title><h5>Evolution State</h5></v-list-tile-title>
                                            </v-list-tile>
                                            <v-list-tile v-for="(value, state) in selectedTask.evolutionState"
                                                         :key="state">
                                                <v-list-tile-content>{{ state }}</v-list-tile-content>
                                                <v-list-tile-content class="align-end">{{ value }}</v-list-tile-content>
                                            </v-list-tile>
                                            <v-divider></v-divider>
                                            <v-list-tile>
                                                <v-list-tile-title><h5>Population Capacity</h5></v-list-tile-title>
                                            </v-list-tile>
                                            <v-list-tile v-for="(value, capacity) in selectedTask.capacity"
                                                         :key="capacity">
                                                <v-list-tile-content>{{ capacity }}</v-list-tile-content>
                                                <v-list-tile-content class="align-end">{{ value }}</v-list-tile-content>
                                            </v-list-tile>
                                            <v-divider></v-divider>
                                            <v-list-tile>
                                                <v-list-tile-title><h5>Coefficients</h5></v-list-tile-title>
                                            </v-list-tile>
                                            <v-list-tile v-for="(value, coefficient) in selectedTask.coefficients"
                                                         :key="coefficient">
                                                <v-list-tile-content>{{ coefficient }}</v-list-tile-content>
                                                <v-list-tile-content class="align-end">{{ value }}</v-list-tile-content>
                                            </v-list-tile>
                                        </v-list>
                                    </v-card>
                                </v-flex>
                                <v-flex grow>
                                    <v-card flat>
                                        <v-list dense>
                                            <v-list-tile>
                                                <v-list-tile-content>
                                                    <v-list-tile-title><h3>Detail Process</h3></v-list-tile-title>
                                                </v-list-tile-content>
                                            </v-list-tile>
                                            <template v-if="selectedTask.bestChromosome">
                                                <v-list-tile>
                                                    <v-list-tile-title><h4>Best Chromosome</h4></v-list-tile-title>
                                                </v-list-tile>
                                                <v-list-tile>
                                                    <v-list-tile-content>Fitness</v-list-tile-content>
                                                    <v-list-tile-content class="align-end">{{ selectedTask.bestChromosome.fitness }}</v-list-tile-content>
                                                </v-list-tile>
                                                <v-list-tile v-for="(value, objective) in selectedTask.bestChromosome.objectiveValues"
                                                             :key="objective">
                                                    <v-list-tile-content>{{ objective }}</v-list-tile-content>
                                                    <v-list-tile-content class="align-end">{{ value }}</v-list-tile-content>
                                                </v-list-tile>
                                            </template>
                                        </v-list>
                                    </v-card>
                                </v-flex>
                                <v-flex style="width: 70px !important;">
                                    <v-card flat>
                                        <v-list dense>
                                            <v-list-tile>
                                                <v-list-tile-content>
                                                    <v-list-tile-title><h3>Actions</h3></v-list-tile-title>
                                                </v-list-tile-content>
                                            </v-list-tile>
                                            <v-list-tile>
                                                <v-list-tile-title><h4>Termination</h4></v-list-tile-title>
                                            </v-list-tile>
                                            <v-radio-group v-model="termination.kind">
                                                <v-list-tile v-for="(name, kind) in termination.kinds" :key="name">
                                                    <v-radio :key="kind"
                                                             :label="name"
                                                             :value="kind"
                                                    ></v-radio>
                                                </v-list-tile>
                                            </v-radio-group>
                                            <v-list-tile v-if="termination.kind > 0">
                                                <v-text-field :label="termination.kinds[termination.kind]"
                                                              v-model.number="termination.value"
                                                              type="number"
                                                              outline
                                                ></v-text-field>
                                            </v-list-tile>
                                            <v-list-tile>
                                                <v-btn depressed outline block @click="toggleEvolution">
                                                    {{ selectedTask.isRunning ? 'Stop' : 'Start' }}
                                                </v-btn>
                                            </v-list-tile>
                                        </v-list>
                                    </v-card>
                                </v-flex>
                            </v-layout>
                        </v-container>
                    </v-card>
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
                selectedTask: null,
                termination: {
                    kind: 0,
                    value: 0,
                    kinds: ["Manual", "Time Limit", "Evolution Count Limit"]
                }
            }
        },
        mounted() {
            axios.get('http://localhost:5000/api/data/group')
                .then(result => {
                    this.groups = result.data;
                    if (this.groups.length > 0) {
                        this.$router.push(`/${this.groups[0].id}`)
                    }
                });

            axios.get('http://localhost:5000/api/ga')
                .then(result => {
                    this.tasks = result.data
                });

            this.notification.start().then(function () {
                console.log("notification connected");
            });

            const commonNotifications = [
                "BuildingTask",
                "BuildingRepositoryTask",
                "TaskBuildFinished",
                "TaskRemoved",
                "RunningTask",
                "TaskIsRunning",
                "StoppingTask",
            ];

            commonNotifications.forEach(kind => {
                this.notification.on(kind, taskId => {
                    const task = this.tasks.find(task => task.id === taskId);
                    task.state = kind;
                })
            });

            this.notification.on("EvolvedOnce", (taskId, state, best) => {
                const task = this.tasks.find(task => task.id === taskId);
                task.evolutionState = state;
                task.bestChromosome = best;
                // console.log(best);
            });

            this.notification.on("TaskFinished", (taskId, state) => {
                const task = this.tasks.find(task => task.id === taskId);
                task.evolutionState = state;
                task.isRunning = false;
                task.state = "Finished";
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
                    capacity: this.newTask.capacity
                }).then(result => {
                    this.selectedTask = result.data;
                    this.tasks.push(result.data);
                    this.newTask = null;
                    this.states.creatingTask = false;
                })
            },

            toggleEvolution() {
                if (this.selectedTask == null) return;
                if (this.selectedTask.isRunning) {
                    this.stopEvolution();
                    return;
                }
                this.startEvolution();
            },

            startEvolution() {
                if (this.selectedTask == null) return;
                let termination = {
                    kind: this.termination.kind,
                    value: this.termination.value,
                };
                axios.post(`http://localhost:5000/api/ga/${this.selectedTask.id}`, termination).then(result => {
                    this.selectedTask.isRunning = true;
                });
            },
            stopEvolution() {
                axios.patch(`http://localhost:5000/api/ga/${this.selectedTask.id}`)
                    .then(result => {
                    })
            },
            generateData() {
                axios.post('http://localhost:5000/api/data/generate')
                    .then(() => {})
            }
        }
    }
</script>
