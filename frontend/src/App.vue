<script setup lang="ts">
import { onMounted } from 'vue'
import { state } from './state'

function loadRates() {
  fetch('/api/rates')
    .then((r) => r.json())
    .then((data) => {
      state.rates = data
      state.lastUpdated = new Date().toLocaleTimeString()
    })
}

function loadAlerts() {
  fetch('/api/alerts')
    .then((r) => r.json())
    .then((data) => {
      state.alerts = data
    })
}

function createAlert() {
  if (!state.newAlert.threshold) {
    alert('Please enter a threshold')
    return
  }

  fetch('/api/alerts', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      pair: state.newAlert.pair,
      threshold: parseFloat(state.newAlert.threshold),
      direction: state.newAlert.direction,
    }),
  })
    .then((r) => {
      if (!r.ok) throw new Error('Failed to create alert')
      return r.json()
    })
    .then(() => {
      state.newAlert = { pair: 'USD/CAD', threshold: '', direction: 'above' }
      state.showAlertForm = false
      loadAlerts()
    })
    .catch((e) => alert('Error: ' + e.message))
}

function deleteAlert(id: string) {
  fetch(`/api/alerts/${id}`, { method: 'DELETE' })
    .then((r) => {
      if (!r.ok) throw new Error('Failed to delete alert')
      loadAlerts()
    })
    .catch((e) => alert('Error: ' + e.message))
}

function getTriggeredAlerts() {
  return state.alerts.filter((a) => a.triggered)
}

function getUsdCad() {
  for (let i = 0; i < state.rates.length; i++) {
    if (state.rates[i].pair === 'USD/CAD') {
      return state.rates[i].rate.toFixed(4)
    }
  }
  return '...'
}

function getGbpUsd() {
  for (let i = 0; i < state.rates.length; i++) {
    if (state.rates[i].pair === 'GBP/USD') {
      return state.rates[i].rate.toFixed(4)
    }
  }
  return '...'
}

function getEurUsd() {
  for (let i = 0; i < state.rates.length; i++) {
    if (state.rates[i].pair === 'EUR/USD') {
      return state.rates[i].rate.toFixed(4)
    }
  }
  return '...'
}

onMounted(() => {
  loadRates()
  loadAlerts()
})

defineExpose({ loadRates, loadAlerts })
</script>

<template>
  <main class="page">
    <header class="header">
      <h1>Xe Rate Board</h1>
      <span class="updated" v-if="state.lastUpdated">Last updated {{ state.lastUpdated }}</span>
    </header>

    <section class="cards">
      <div class="card">
        <div class="pair">USD / CAD</div>
        <div class="rate">{{ getUsdCad() }}</div>
        <div class="caption">1 US dollar in Canadian dollars</div>
      </div>

      <div class="card">
        <div class="pair">GBP / USD</div>
        <div class="rate">{{ getGbpUsd() }}</div>
        <div class="caption">1 British pound in US dollars</div>
      </div>

      <div class="card">
        <div class="pair">EUR / USD</div>
        <div class="rate">{{ getEurUsd() }}</div>
        <div class="caption">1 euro in US dollars</div>
      </div>
    </section>

    <button class="refresh" @click="loadRates">Refresh rates</button>

    <!-- Alerts Section -->
    <section class="alerts-section">
      <h2>Rate Alerts</h2>

      <!-- Triggered Alerts -->
      <div v-if="getTriggeredAlerts().length > 0" class="alerts-triggered">
        <h3 class="triggered-heading">⚠️ Triggered Alerts</h3>
        <div v-for="alert in getTriggeredAlerts()" :key="alert.id" class="alert alert-active">
          <div class="alert-content">
            <div class="alert-pair">{{ alert.pair }}</div>
            <div class="alert-details">
              When {{ alert.direction }} {{ alert.threshold }} → <strong>TRIGGERED</strong>
            </div>
          </div>
          <button class="btn-delete" @click="deleteAlert(alert.id)">Delete</button>
        </div>
      </div>

      <!-- All Alerts -->
      <div class="alerts-all">
        <h3>All Alerts ({{ state.alerts.length }})</h3>
        <div v-if="state.alerts.length === 0" class="empty-message">No alerts yet. Create one below.</div>
        <div v-for="alert in state.alerts" :key="alert.id" :class="{ 'alert-active': alert.triggered }">
          <div class="alert-content">
            <div class="alert-pair">{{ alert.pair }}</div>
            <div class="alert-details">
              When {{ alert.direction }} {{ alert.threshold }}
              <span v-if="alert.triggered" class="triggered-badge">TRIGGERED</span>
            </div>
          </div>
          <button class="btn-delete" @click="deleteAlert(alert.id)">Delete</button>
        </div>
      </div>

      <!-- Create Alert Form -->
      <button v-if="!state.showAlertForm" class="btn-create-alert" @click="state.showAlertForm = true">
        + Create Alert
      </button>

      <form v-if="state.showAlertForm" class="alert-form" @submit.prevent="createAlert">
        <div class="form-group">
          <label>Currency Pair</label>
          <select v-model="state.newAlert.pair">
            <option value="USD/CAD">USD/CAD</option>
            <option value="GBP/USD">GBP/USD</option>
            <option value="EUR/USD">EUR/USD</option>
          </select>
        </div>

        <div class="form-group">
          <label>Threshold Rate</label>
          <input v-model="state.newAlert.threshold" type="number" step="0.0001" placeholder="e.g., 1.3650" />
        </div>

        <div class="form-group">
          <label>Alert When Rate Goes</label>
          <select v-model="state.newAlert.direction">
            <option value="above">Above</option>
            <option value="below">Below</option>
          </select>
        </div>

        <div class="form-actions">
          <button type="submit" class="btn-create">Create Alert</button>
          <button type="button" class="btn-cancel" @click="state.showAlertForm = false">Cancel</button>
        </div>
      </form>
    </section>
  </main>
</template>

<style>
* {
  box-sizing: border-box;
}

body {
  margin: 0;
  font-family: 'Segoe UI', system-ui, sans-serif;
  background: #f4f6f8;
  color: #1a2233;
}

.page {
  max-width: 860px;
  margin: 0 auto;
  padding: 32px 20px;
}

.header {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  margin-bottom: 24px;
}

h1 {
  font-size: 1.6rem;
  margin: 0;
}

h2 {
  font-size: 1.3rem;
  margin: 32px 0 16px 0;
  color: #16345c;
}

h3 {
  font-size: 1rem;
  margin: 12px 0;
  color: #1a2233;
}

.updated {
  font-size: 0.85rem;
  color: #66718a;
}

.cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
}

.card {
  background: #ffffff;
  border: 1px solid #e1e6ee;
  border-radius: 10px;
  padding: 20px;
}

.pair {
  font-size: 0.9rem;
  font-weight: 600;
  color: #66718a;
  letter-spacing: 0.04em;
}

.rate {
  font-size: 2rem;
  font-weight: 700;
  margin: 8px 0 4px;
  font-variant-numeric: tabular-nums;
}

.caption {
  font-size: 0.8rem;
  color: #8a93a8;
}

.refresh {
  margin-top: 24px;
  padding: 10px 18px;
  border: none;
  border-radius: 8px;
  background: #16345c;
  color: #ffffff;
  font-size: 0.9rem;
  cursor: pointer;
}

.refresh:hover {
  background: #1d4377;
}

/* Alerts Section */
.alerts-section {
  margin-top: 40px;
  padding-top: 32px;
  border-top: 2px solid #e1e6ee;
}

.alerts-triggered {
  margin-bottom: 24px;
  padding: 16px;
  background: #fef5f5;
  border-left: 4px solid #e85d5d;
  border-radius: 8px;
}

.triggered-heading {
  color: #c41e3a;
  margin-top: 0;
}

.alerts-all {
  margin-bottom: 24px;
}

.alert {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px;
  background: #ffffff;
  border: 1px solid #e1e6ee;
  border-radius: 6px;
  margin-bottom: 8px;
  transition: background 0.2s;
}

.alert.alert-active {
  background: #fff3cd;
  border-color: #ffc107;
}

.alert-content {
  flex: 1;
}

.alert-pair {
  font-weight: 600;
  margin-bottom: 4px;
  font-size: 0.95rem;
}

.alert-details {
  font-size: 0.85rem;
  color: #66718a;
}

.triggered-badge {
  display: inline-block;
  margin-left: 8px;
  padding: 2px 8px;
  background: #c41e3a;
  color: white;
  border-radius: 3px;
  font-weight: 600;
  font-size: 0.75rem;
}

.btn-delete {
  padding: 6px 12px;
  background: #e85d5d;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.85rem;
  margin-left: 12px;
}

.btn-delete:hover {
  background: #c41e3a;
}

.empty-message {
  padding: 16px;
  color: #8a93a8;
  font-style: italic;
  text-align: center;
}

.btn-create-alert {
  padding: 10px 16px;
  background: #16345c;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 0.9rem;
  font-weight: 600;
}

.btn-create-alert:hover {
  background: #1d4377;
}

.alert-form {
  background: #f9fafc;
  border: 1px solid #e1e6ee;
  border-radius: 8px;
  padding: 16px;
  margin-top: 16px;
}

.form-group {
  margin-bottom: 16px;
}

.form-group:last-of-type {
  margin-bottom: 0;
}

.form-group label {
  display: block;
  font-size: 0.85rem;
  font-weight: 600;
  color: #1a2233;
  margin-bottom: 6px;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #e1e6ee;
  border-radius: 4px;
  font-size: 0.9rem;
  font-family: inherit;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: #16345c;
  box-shadow: 0 0 0 2px rgba(22, 52, 92, 0.1);
}

.form-actions {
  display: flex;
  gap: 12px;
  margin-top: 16px;
}

.btn-create,
.btn-cancel {
  flex: 1;
  padding: 10px 16px;
  border: none;
  border-radius: 6px;
  font-size: 0.9rem;
  cursor: pointer;
  font-weight: 600;
}

.btn-create {
  background: #16345c;
  color: white;
}

.btn-create:hover {
  background: #1d4377;
}

.btn-cancel {
  background: #e1e6ee;
  color: #1a2233;
}

.btn-cancel:hover {
  background: #d1d8e8;
}
</style>
