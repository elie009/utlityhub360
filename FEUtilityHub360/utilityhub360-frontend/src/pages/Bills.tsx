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
  Receipt as ReceiptIcon,
  Download as DownloadIcon,
  Visibility as ViewIcon,
} from '@mui/icons-material';
import { DataGrid, GridColDef, GridActionsCellItem } from '@mui/x-data-grid';

interface Bill {
  id: number;
  utilityName: string;
  amount: number;
  dueDate: string;
  status: 'Paid' | 'Pending' | 'Overdue';
  billDate: string;
  accountNumber: string;
}

const Bills: React.FC = () => {
  const [open, setOpen] = useState(false);
  const [bills, setBills] = useState<Bill[]>([
    {
      id: 1,
      utilityName: 'Main Electricity',
      amount: 120.50,
      dueDate: '2024-02-15',
      status: 'Pending',
      billDate: '2024-01-15',
      accountNumber: 'ELC-001',
    },
    {
      id: 2,
      utilityName: 'Water Supply',
      amount: 45.75,
      dueDate: '2024-02-10',
      status: 'Paid',
      billDate: '2024-01-10',
      accountNumber: 'WTR-002',
    },
    {
      id: 3,
      utilityName: 'Natural Gas',
      amount: 89.25,
      dueDate: '2024-01-25',
      status: 'Overdue',
      billDate: '2024-01-01',
      accountNumber: 'GAS-003',
    },
    {
      id: 4,
      utilityName: 'Internet Service',
      amount: 65.00,
      dueDate: '2024-02-20',
      status: 'Pending',
      billDate: '2024-01-20',
      accountNumber: 'INT-004',
    },
  ]);

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Paid':
        return 'success';
      case 'Pending':
        return 'warning';
      case 'Overdue':
        return 'error';
      default:
        return 'default';
    }
  };

  const handleEdit = (id: number) => {
    console.log('Edit bill:', id);
  };

  const handleDelete = (id: number) => {
    setBills(bills.filter(bill => bill.id !== id));
  };

  const handleView = (id: number) => {
    console.log('View bill:', id);
  };

  const handleDownload = (id: number) => {
    console.log('Download bill:', id);
  };

  const handleAddBill = () => {
    setOpen(true);
  };

  const handleClose = () => {
    setOpen(false);
  };

  const columns: GridColDef[] = [
    {
      field: 'utilityName',
      headerName: 'Utility',
      width: 200,
      renderCell: (params) => (
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <ReceiptIcon sx={{ mr: 1, color: 'primary.main' }} />
          {params.value}
        </Box>
      ),
    },
    { field: 'accountNumber', headerName: 'Account #', width: 120 },
    {
      field: 'amount',
      headerName: 'Amount',
      width: 120,
      renderCell: (params) => (
        <Typography variant="body2" fontWeight="bold">
          ${params.value.toFixed(2)}
        </Typography>
      ),
    },
    { field: 'billDate', headerName: 'Bill Date', width: 120 },
    { field: 'dueDate', headerName: 'Due Date', width: 120 },
    {
      field: 'status',
      headerName: 'Status',
      width: 120,
      renderCell: (params) => (
        <Chip
          label={params.value}
          color={getStatusColor(params.value) as any}
          size="small"
        />
      ),
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: 'Actions',
      width: 150,
      getActions: (params) => [
        <GridActionsCellItem
          icon={<ViewIcon />}
          label="View"
          onClick={() => handleView(params.row.id)}
        />,
        <GridActionsCellItem
          icon={<DownloadIcon />}
          label="Download"
          onClick={() => handleDownload(params.row.id)}
        />,
        <GridActionsCellItem
          icon={<EditIcon />}
          label="Edit"
          onClick={() => handleEdit(params.row.id)}
        />,
        <GridActionsCellItem
          icon={<DeleteIcon />}
          label="Delete"
          onClick={() => handleDelete(params.row.id)}
        />,
      ],
    },
  ];

  const totalAmount = bills.reduce((sum, bill) => sum + bill.amount, 0);
  const paidAmount = bills.filter(bill => bill.status === 'Paid').reduce((sum, bill) => sum + bill.amount, 0);
  const pendingAmount = bills.filter(bill => bill.status === 'Pending').reduce((sum, bill) => sum + bill.amount, 0);
  const overdueAmount = bills.filter(bill => bill.status === 'Overdue').reduce((sum, bill) => sum + bill.amount, 0);

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">
          Bills Management
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={handleAddBill}
        >
          Add Bill
        </Button>
      </Box>

      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Total Amount
              </Typography>
              <Typography variant="h4">
                ${totalAmount.toFixed(2)}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Paid Amount
              </Typography>
              <Typography variant="h4" color="success.main">
                ${paidAmount.toFixed(2)}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Pending Amount
              </Typography>
              <Typography variant="h4" color="warning.main">
                ${pendingAmount.toFixed(2)}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Overdue Amount
              </Typography>
              <Typography variant="h4" color="error.main">
                ${overdueAmount.toFixed(2)}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Paper sx={{ height: 400, width: '100%' }}>
        <DataGrid
          rows={bills}
          columns={columns}
          pageSizeOptions={[5, 10, 25]}
          initialState={{
            pagination: {
              paginationModel: { page: 0, pageSize: 5 },
            },
          }}
          checkboxSelection
          disableRowSelectionOnClick
        />
      </Paper>

      <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
        <DialogTitle>Add New Bill</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Utility</InputLabel>
                <Select
                  label="Utility"
                >
                  <MenuItem value="Main Electricity">Main Electricity</MenuItem>
                  <MenuItem value="Water Supply">Water Supply</MenuItem>
                  <MenuItem value="Natural Gas">Natural Gas</MenuItem>
                  <MenuItem value="Internet Service">Internet Service</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Account Number"
                variant="outlined"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Amount"
                type="number"
                variant="outlined"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Bill Date"
                type="date"
                variant="outlined"
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Due Date"
                type="date"
                variant="outlined"
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Cancel</Button>
          <Button onClick={handleClose} variant="contained">
            Add Bill
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Bills;
