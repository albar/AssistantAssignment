<template>
    <div>
        <v-navigation-drawer
                permanent
                width="150"
                fixed
                right
                app
        >
            <v-toolbar flat>
                <v-list>
                    <v-list-tile>
                        <v-list-tile-title class="title">
                            Subjects
                        </v-list-tile-title>
                    </v-list-tile>
                </v-list>
            </v-toolbar>

            <v-divider></v-divider>

            <v-list>
                <v-list-tile
                        v-for="subject in subjects"
                        :key="subject.id"
                        @click="select(subject)"
                >
                    <v-list-tile-content>
                        <v-list-tile-title>{{ subject.code || `Subject ${subject.id}` }}</v-list-tile-title>
                    </v-list-tile-content>
                </v-list-tile>
            </v-list>
        </v-navigation-drawer>

        <v-card flat v-if="group">
            <v-toolbar flat>
                <v-list>
                    <v-list-tile>
                        <v-list-tile-title class="title">
                            Data Group "{{ group.name || `Data ${group.id}` }}"
                        </v-list-tile-title>
                    </v-list-tile>
                </v-list>
            </v-toolbar>

            <v-divider></v-divider>
            
            <div v-if="selectedSubject">
                <v-card flat>
                    <v-toolbar flat>
                        <v-list>
                            <v-list-tile>
                                <v-list-tile-title class="title">
                                    {{ selectedSubject.code }}
                                </v-list-tile-title>

                                <v-chip color="blue" text-color="white">
                                    <v-avatar class="blue darken-2">{{ selectedSubject.assistantPerScheduleCount }}
                                    </v-avatar>
                                    Assistants Required Per Schedule
                                </v-chip>
                            </v-list-tile>
                        </v-list>
                    </v-toolbar>
                    <v-card-title>
                        <v-chip color="green" text-color="white" v-for="assessment in assessments" :key="assessment.value">
                            <v-avatar class="green darken-2">
                                {{ selectedSubject.assessmentsThreshold[assessment.value] }}
                            </v-avatar>
                            {{ assessment.text }}
                        </v-chip>
                    </v-card-title>
                    <v-divider></v-divider>
                    <v-tabs>
                        <v-tab>
                            Schedules
                        </v-tab>
                        <v-tab>
                            Assistants
                        </v-tab>
                        <v-tab-item>
                            <v-data-table
                                    :headers="scheduleHeader"
                                    :items="selectedSubject.schedules"
                                    hide-actions
                            >
                                <template v-slot:items="props">
                                    <td>{{ props.index + 1 }}</td>
                                    <td>{{ props.item.day }}</td>
                                    <td>{{ props.item.session }}</td>
                                    <td>{{ props.item.lab }}</td>
                                </template>
                            </v-data-table>
                        </v-tab-item>
                        <v-tab-item>
                            <v-data-table
                                    :headers="assistantHeader"
                                    :items="selectedSubject.assistants"
                                    hide-actions
                            >
                                <template v-slot:items="props">
                                    <td>{{ props.index + 1 }}</td>
                                    <td>{{ props.item.npm }}</td>
                                    <td class="text-xs-center" v-for="assessment in assessments" :key="assessment.value">{{
                                        props.item.assessment[assessment.value] }}
                                    </td>
                                </template>
                            </v-data-table>
                        </v-tab-item>
                    </v-tabs>
                </v-card>
            </div>
        </v-card>
    </div>
</template>

<script>
    import axios from 'axios'

    export default {
        name: "detail",
        data() {
            return {
                group: null,
                subjects: [],
                selectedSubject: null,
            }
        },
        mounted() {
            this.loadData();
        },
        watch: {
            '$route'()  {
                this.loadData();
            }
        },
        computed: {
            scheduleHeader() {
                if (!this.selectedSubject) return []

                return [
                    {text: '', value: ''},
                    {text: 'Day', value: 'day'},
                    {text: 'Session', value: 'sessios'},
                    {text: 'Lab', value: 'lab'}
                ]
            },

            assistantHeader() {
                if (!this.selectedSubject) return []

                return [
                    {text: '', value: ''},
                    {text: 'Npm', value: 'npm'},
                    ...this.assessments
                ]
            },

            assessments() {
                if (!this.selectedSubject) return []
                return Object.keys(this.selectedSubject.assessmentsThreshold).map(ass => ({text: ass, value: ass}))
            }
        },
        methods: {
            loadData() {
                axios.get(`http://localhost:5000/api/data/group/${this.$route.params.id}/subject?assistants=true&schedules=true`)
                    .then(result => {
                        this.group = result.data.group
                        this.subjects = result.data.subjects
                        if (this.subjects.length > 0) {
                            this.selectedSubject = this.subjects[0]
                        }
                    })
            },
            select(subject) {
                this.selectedSubject = subject
            },
        }
    }
</script>

<style scoped>
    .row {
        display: flex;
    }

    .column {
        flex: 50%;
    }
</style>