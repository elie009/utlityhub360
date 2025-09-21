import React, { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Button,
  Card,
  CardContent,
  Grid,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  ElectricBolt as ElectricIcon,
  WaterDrop as WaterIcon,
  GasMeter as GasIcon,
  Wifi as InternetIcon,
} from '@mui/icons-material';

interface Utility {
  id: number;
  name: string;
  type: 'Electricity' | 'Water' | 'Gas' | 'Internet';
  provider: string;
  status: 'Active' | 'Inactive' | 'Maintenance';
  monthlyCost: number;
  lastReading: string;
}

const Utilities: React.FC = () => {
  const [open, setOpen] = useState(false);
  const [utilities, setUtilities] = useState<Utility[]>([
    {
      id: 1,
      name: 'Main Electricity',
      type: 'Electricity',
      provider: 'PowerCorp Inc.',
      status: 'Active',
      monthlyCost: 120.50,
      lastReading: '2024-01-15',
    },
    {
      id: 2,
      name: 'Water Supply',
      type: 'Water',
      provider: 'AquaFlow Ltd.',
      status: 'Active',
      monthlyCost: 45.75,
      lastReading: '2024-01-14',
    },
    {
      id: 3,
      name: 'Natural Gas',
      type: 'Gas',
      provider: 'GasMax Energy',
      status: 'Maintenance',
      monthlyCost: 89.25,
      lastReading: '2024-01-10',
    },
    {
      id: 4,
      name: 'Internet Service',
      type: 'Internet',
      provider: 'NetConnect',
      status: 'Active',
      monthlyCost: 65.00,
      lastReading: '2024-01-16',
    },
  ]);

  const getUtilityIcon = (type: string) => {
    switch (type) {
      case 'Electricity':
        return <ElectricIcon sx={{ fontSize: 40, color: 'warning.main' }} />;
      case 'Water':
        return <WaterIcon sx={{ fontSize: 40, color: 'info.main' }} />;
      case 'Gas':
        return <GasIcon sx={{ fontSize: 40, color: 'error.main' }} />;
      case 'Internet':
        return <InternetIcon sx={{ fontSize: 40, color: 'primary.main' }} />;
      default:
        return <ElectricIcon sx={{ fontSize: 40 }} />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Active':
        return 'success';
      case 'Inactive':
        return 'default';
      case 'Maintenance':
        return 'warning';
      default:
        return 'default';
    }
  };

  const handleEdit = (id: number) => {
    console.log('Edit utility:', id);
  };

  const handleDelete = (id: number) => {
    setUtilities(utilities.filter(utility => utility.id !== id));
  };

  const handleAddUtility = () => {
    setOpen(true);
  };

  const handleClose = () => {
    setOpen(false);
  };

  const totalMonthlyCost = utilities.reduce((sum, utility) => sum + utility.monthlyCost, 0);

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">
          Utilities Management
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={handleAddUtility}
        >
          Add Utility
        </Button>
      </Box>

      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Total Utilities
              </Typography>
              <Typography variant="h4">
                {utilities.length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Active Utilities
              </Typography>
              <Typography variant="h4">
                {utilities.filter(utility => utility.status === 'Active').length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Total Monthly Cost
              </Typography>
              <Typography variant="h4">
                ${totalMonthlyCost.toFixed(2)}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Maintenance Required
              </Typography>
              <Typography variant="h4">
                {utilities.filter(utility => utility.status === 'Maintenance').length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Grid container spacing={3}>
        {utilities.map((utility) => (
          <Grid item xs={12} sm={6} md={4} key={utility.id}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                  <Box>
                    <Typography variant="h6" gutterBottom>
                      {utility.name}
                    </Typography>
                    <Typography color="textSecondary" variant="body2">
                      {utility.provider}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <IconButton size="small" onClick={() => handleEdit(utility.id)}>
                      <EditIcon />
                    </IconButton>
                    <IconButton size="small" onClick={() => handleDelete(utility.id)}>
                      <DeleteIcon />
                    </IconButton>
                  </Box>
                </Box>
                
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  {getUtilityIcon(utility.type)}
                  <Box sx={{ ml: 2 }}>
                    <Typography variant="body2" color="textSecondary">
                      {utility.type}
                    </Typography>
                    <Chip
                      label={utility.status}
                      color={getStatusColor(utility.status) as any}
                      size="small"
                    />
                  </Box>
                </Box>

                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography variant="body2" color="textSecondary">
                    Monthly Cost:
                  </Typography>
                  <Typography variant="body2" fontWeight="bold">
                    ${utility.monthlyCost.toFixed(2)}
                  </Typography>
                </Box>
                
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="textSecondary">
                    Last Reading:
                  </Typography>
                  <Typography variant="body2">
                    {utility.lastReading}
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
        <DialogTitle>Add New Utility</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Utility Name"
                variant="outlined"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Type</InputLabel>
                <Select
                  label="Type"
                >
                  <MenuItem value="Electricity">Electricity</MenuItem>
                  <MenuItem value="Water">Water</MenuItem>
                  <MenuItem value="Gas">Gas</MenuItem>
                  <MenuItem value="Internet">Internet</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Provider"
                variant="outlined"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Monthly Cost"
                type="number"
                variant="outlined"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Cancel</Button>
          <Button onClick={handleClose} variant="contained">
            Add Utility
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Utilities;
