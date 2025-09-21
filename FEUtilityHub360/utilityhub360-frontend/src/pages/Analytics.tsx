import React from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Card,
  CardContent,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import {
  LineChart,
  Line,
  AreaChart,
  Area,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

const Analytics: React.FC = () => {
  const monthlyData = [
    { name: 'Jan', users: 4000, bills: 2400, revenue: 12000 },
    { name: 'Feb', users: 3000, bills: 1398, revenue: 15000 },
    { name: 'Mar', users: 2000, bills: 9800, revenue: 18000 },
    { name: 'Apr', users: 2780, bills: 3908, revenue: 22000 },
    { name: 'May', users: 1890, bills: 4800, revenue: 25000 },
    { name: 'Jun', users: 2390, bills: 3800, revenue: 28000 },
  ];

  const utilityData = [
    { name: 'Electricity', value: 35, color: '#8884d8' },
    { name: 'Water', value: 25, color: '#82ca9d' },
    { name: 'Gas', value: 20, color: '#ffc658' },
    { name: 'Internet', value: 20, color: '#ff7300' },
  ];

  const yearlyData = [
    { year: '2020', revenue: 120000, users: 5000 },
    { year: '2021', revenue: 150000, users: 7500 },
    { year: '2022', revenue: 180000, users: 10000 },
    { year: '2023', revenue: 220000, users: 12500 },
    { year: '2024', revenue: 280000, users: 15000 },
  ];

  const kpiData = [
    {
      title: 'Total Revenue',
      value: '$280,000',
      change: '+12.5%',
      trend: 'up',
    },
    {
      title: 'Active Users',
      value: '15,000',
      change: '+8.2%',
      trend: 'up',
    },
    {
      title: 'Bills Processed',
      value: '45,678',
      change: '+15.3%',
      trend: 'up',
    },
    {
      title: 'Avg. Bill Amount',
      value: '$125.50',
      change: '+3.1%',
      trend: 'up',
    },
  ];

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">
          Analytics Dashboard
        </Typography>
        <FormControl sx={{ minWidth: 120 }}>
          <InputLabel>Time Period</InputLabel>
          <Select value="6months" label="Time Period">
            <MenuItem value="1month">1 Month</MenuItem>
            <MenuItem value="3months">3 Months</MenuItem>
            <MenuItem value="6months">6 Months</MenuItem>
            <MenuItem value="1year">1 Year</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {/* KPI Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        {kpiData.map((kpi, index) => (
          <Grid item xs={12} sm={6} md={3} key={index}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom variant="h6">
                  {kpi.title}
                </Typography>
                <Typography variant="h4" component="h2">
                  {kpi.value}
                </Typography>
                <Typography color="textSecondary">
                  {kpi.change} from last period
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Charts */}
      <Grid container spacing={3}>
        {/* Revenue Trend */}
        <Grid item xs={12} md={8}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Revenue Trend
            </Typography>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={monthlyData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Line type="monotone" dataKey="revenue" stroke="#1976d2" strokeWidth={2} />
              </LineChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>

        {/* Utility Distribution */}
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Utility Distribution
            </Typography>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={utilityData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="value"
                >
                  {utilityData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>

        {/* User Growth */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              User Growth
            </Typography>
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={monthlyData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Area type="monotone" dataKey="users" stroke="#82ca9d" fill="#82ca9d" />
              </AreaChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>

        {/* Bills Processed */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Bills Processed
            </Typography>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={monthlyData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Bar dataKey="bills" fill="#ffc658" />
              </BarChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>

        {/* Yearly Comparison */}
        <Grid item xs={12}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Yearly Performance Comparison
            </Typography>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={yearlyData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="year" />
                <YAxis yAxisId="left" />
                <YAxis yAxisId="right" orientation="right" />
                <Tooltip />
                <Legend />
                <Bar yAxisId="left" dataKey="revenue" fill="#1976d2" name="Revenue ($)" />
                <Bar yAxisId="right" dataKey="users" fill="#82ca9d" name="Users" />
              </BarChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Analytics;
