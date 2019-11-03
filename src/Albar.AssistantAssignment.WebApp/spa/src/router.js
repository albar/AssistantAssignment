import Vue from 'vue'
import Router from 'vue-router'
import Home from './views/Home.vue'
import DetailGroup from './views/GroupDetail'

Vue.use(Router)

export default new Router({
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home
    },
    {
      path: '/:id',
      name: 'group',
      component: DetailGroup
    }
  ]
})
