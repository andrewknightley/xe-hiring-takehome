import { reactive } from 'vue'

// quick and dirty shared state, works fine for now
export const state = reactive({
  rates: [] as any[],
  alerts: [] as any[],
  lastUpdated: '',
  showAlertForm: false,
  newAlert: { pair: 'USD/CAD', threshold: '', direction: 'above' },
})
