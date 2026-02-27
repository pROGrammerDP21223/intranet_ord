import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import Layout from '../components/Layout';
import { clientAPI, serviceAPI } from '../services/api';
import { useRole } from '../hooks/useRole';

const EditClient = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const role = useRole();
  const canvasRef = useRef(null);
  const [isDrawing, setIsDrawing] = useState(false);
  const [services, setServices] = useState([]);
  const [loadingServices, setLoadingServices] = useState(true);
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});
  const [success, setSuccess] = useState(false);

  // Check if user can manage WhatsApp/Email settings
  const canManageNotifications = role.isAdmin || role.isOwner || role.isSalesPerson || role.isSalesManager;

  const [formData, setFormData] = useState({
    customerNo: '',
    formDate: new Date().toISOString().split('T')[0],
    amountWithoutGst: '',
    includeGst: true,
    gstPercentage: '18',
    gstAmount: '',
    totalPackage: '',
    companyName: '',
    contactPerson: '',
    designation: '',
    address: '',
    phone: '',
    email: '',
    domainName: '',
    gstNo: '',
    serviceIds: [],
    emailServices: [],
    popIdCount: '',
    gSuiteIdCount: '',
    specificGuidelines: '',
    seoKeywordRange: '',
    seoLocation: '',
    seoKeywordsList: '',
    adwordsKeywords: '',
    adwordsPeriod: '',
    adwordsLocation: '',
    adwordsKeywordsList: '',
    specialGuidelines: '',
    nameDesignation: '',
    signature: '',
    // WhatsApp & Email Management
    whatsAppNumber: '',
    enquiryEmail: '',
    useWhatsAppService: false,
    whatsAppSameAsMobile: true,
    useSameEmailForEnquiries: true,
  });

  useEffect(() => {
    loadClient();
    loadServices();
  }, [id]);

  const loadClient = async () => {
    try {
      setLoading(true);
      const response = await clientAPI.getClientById(id);
      if (response.success && response.data) {
        const client = response.data;
        setFormData({
          customerNo: client.customerNo || '',
          formDate: client.formDate ? new Date(client.formDate).toISOString().split('T')[0] : new Date().toISOString().split('T')[0],
          amountWithoutGst: client.amountWithoutGst?.toString() || '',
          includeGst: client.gstPercentage > 0,
          gstPercentage: client.gstPercentage?.toString() || '18',
          gstAmount: client.gstAmount?.toString() || '',
          totalPackage: client.totalPackage?.toString() || '',
          companyName: client.companyName || '',
          contactPerson: client.contactPerson || '',
          designation: client.designation || '',
          address: client.address || '',
          phone: client.phone || '',
          email: client.email || '',
          whatsAppNumber: client.whatsAppNumber || '',
          enquiryEmail: client.enquiryEmail || '',
          useWhatsAppService: client.useWhatsAppService || false,
          whatsAppSameAsMobile: client.whatsAppSameAsMobile !== undefined ? client.whatsAppSameAsMobile : true,
          useSameEmailForEnquiries: client.useSameEmailForEnquiries !== undefined ? client.useSameEmailForEnquiries : true,
          domainName: client.domainName || '',
          gstNo: client.gstNo || '',
          serviceIds: client.clientServices?.map(cs => cs.serviceId) || [],
          emailServices: client.clientEmailServices?.map(es => es.emailServiceType) || [],
          popIdCount: client.clientEmailServices?.find(es => es.emailServiceType === 'pop-id')?.quantity?.toString() || '',
          gSuiteIdCount: client.clientEmailServices?.find(es => es.emailServiceType === 'g-suite-id')?.quantity?.toString() || '',
          specificGuidelines: client.specificGuidelines || '',
          seoKeywordRange: client.clientSeoDetail?.keywordRange || '',
          seoLocation: client.clientSeoDetail?.location || '',
          seoKeywordsList: client.clientSeoDetail?.keywordsList || '',
          adwordsKeywords: client.clientAdwordsDetail?.numberOfKeywords || '',
          adwordsPeriod: client.clientAdwordsDetail?.period || '',
          adwordsLocation: client.clientAdwordsDetail?.location || '',
          adwordsKeywordsList: client.clientAdwordsDetail?.keywordsList || '',
          specialGuidelines: client.clientAdwordsDetail?.specialGuidelines || '',
          nameDesignation: client.nameDesignation || '',
          signature: client.signature || '',
        });
        if (client.eSignature) {
          const img = new Image();
          img.src = client.eSignature;
          img.onload = () => {
            const canvas = canvasRef.current;
            if (canvas) {
              const ctx = canvas.getContext('2d');
              ctx.clearRect(0, 0, canvas.width, canvas.height);
              ctx.drawImage(img, 0, 0);
            }
          };
        }
      }
    } catch (error) {
      console.error('Failed to load client:', error);
      alert('Failed to load client data');
    } finally {
      setLoading(false);
    }
  };

  const loadServices = async () => {
    try {
      setLoadingServices(true);
      const response = await serviceAPI.getServicesByCategory();
      if (response.success && response.data) {
        const flat = response.data.flatMap(cat => cat.services);
        setServices(flat);
      }
    } catch (error) {
      console.error('Failed to load services:', error);
    } finally {
      setLoadingServices(false);
    }
  };

  // Calculate GST - same as ClientForm
  useEffect(() => {
    const amount = parseFloat(formData.amountWithoutGst) || 0;
    const gstPercent = parseFloat(formData.gstPercentage) || 0;
    
    if (formData.includeGst) {
      const gstAmount = amount * (gstPercent / 100);
      const total = amount + gstAmount;
      setFormData(prev => ({
        ...prev,
        gstAmount: gstAmount > 0 ? gstAmount.toFixed(2) : '',
        totalPackage: total > 0 ? total.toFixed(2) : ''
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        gstAmount: '0.00',
        totalPackage: amount > 0 ? amount.toFixed(2) : ''
      }));
    }
  }, [formData.amountWithoutGst, formData.gstPercentage, formData.includeGst]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    
    if (type === 'checkbox') {
      if (name === 'services[]') {
        const serviceId = parseInt(value);
        setFormData(prev => ({
          ...prev,
          serviceIds: checked
            ? [...prev.serviceIds, serviceId]
            : prev.serviceIds.filter(id => id !== serviceId)
        }));
      } else if (name.startsWith('emailServices[]')) {
        const emailServiceType = value;
        setFormData(prev => ({
          ...prev,
          emailServices: checked
            ? [...prev.emailServices, emailServiceType]
            : prev.emailServices.filter(s => s !== emailServiceType)
        }));
      } else if (name === 'whatsAppSameAsMobile') {
        setFormData(prev => ({
          ...prev,
          whatsAppSameAsMobile: checked,
          whatsAppNumber: checked ? '' : prev.whatsAppNumber
        }));
      } else if (name === 'useSameEmailForEnquiries') {
        setFormData(prev => ({
          ...prev,
          useSameEmailForEnquiries: checked,
          enquiryEmail: checked ? '' : prev.enquiryEmail
        }));
      } else {
        setFormData(prev => ({
          ...prev,
          [name]: checked
        }));
      }
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: value
      }));
    }
    
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  // Canvas drawing functions - same as ClientForm
  const startDrawing = (e) => {
    setIsDrawing(true);
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    const rect = canvas.getBoundingClientRect();
    ctx.beginPath();
    ctx.moveTo(e.clientX - rect.left, e.clientY - rect.top);
  };

  const draw = (e) => {
    if (!isDrawing) return;
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    const rect = canvas.getBoundingClientRect();
    ctx.lineTo(e.clientX - rect.left, e.clientY - rect.top);
    ctx.stroke();
  };

  const stopDrawing = () => {
    if (isDrawing) {
      setIsDrawing(false);
      const canvas = canvasRef.current;
      if (canvas) {
        const dataURL = canvas.toDataURL();
        setFormData(prev => ({ ...prev, eSignature: dataURL }));
      }
    }
  };

  const clearSignature = () => {
    const canvas = canvasRef.current;
    if (canvas) {
      const ctx = canvas.getContext('2d');
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      setFormData(prev => ({ ...prev, eSignature: '' }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setSuccess(false);

    try {
      const payload = {
        customerNo: formData.customerNo,
        formDate: formData.formDate,
        amountWithoutGst: parseFloat(formData.amountWithoutGst) || 0,
        gstPercentage: formData.includeGst ? (parseFloat(formData.gstPercentage) || 0) : 0,
        gstAmount: formData.includeGst ? (parseFloat(formData.gstAmount) || 0) : 0,
        totalPackage: parseFloat(formData.totalPackage) || 0,
        companyName: formData.companyName,
        contactPerson: formData.contactPerson,
        designation: formData.designation,
        address: formData.address,
        phone: formData.phone,
        email: formData.email,
        whatsAppNumber: canManageNotifications ? (formData.whatsAppSameAsMobile ? null : formData.whatsAppNumber) : null,
        enquiryEmail: canManageNotifications ? (formData.useSameEmailForEnquiries ? null : formData.enquiryEmail) : null,
        useWhatsAppService: canManageNotifications ? formData.useWhatsAppService : false,
        whatsAppSameAsMobile: canManageNotifications ? formData.whatsAppSameAsMobile : true,
        useSameEmailForEnquiries: canManageNotifications ? formData.useSameEmailForEnquiries : true,
        domainName: formData.domainName,
        gstNo: formData.gstNo,
        specificGuidelines: formData.specificGuidelines,
        nameDesignation: formData.nameDesignation,
        signature: formData.signature,
        eSignature: formData.eSignature,
        services: formData.serviceIds.map(serviceId => ({ serviceId })),
        emailServices: [
          ...(formData.emailServices.includes('pop-id') && formData.popIdCount
            ? [{ emailServiceType: 'pop-id', quantity: parseInt(formData.popIdCount) || 0 }]
            : []),
          ...(formData.emailServices.includes('g-suite-id') && formData.gSuiteIdCount
            ? [{ emailServiceType: 'g-suite-id', quantity: parseInt(formData.gSuiteIdCount) || 0 }]
            : [])
        ],
        seoDetail: {
          keywordRange: formData.seoKeywordRange,
          location: formData.seoLocation,
          keywordsList: formData.seoKeywordsList
        },
        adwordsDetail: {
          numberOfKeywords: formData.adwordsKeywords,
          period: formData.adwordsPeriod,
          location: formData.adwordsLocation,
          keywordsList: formData.adwordsKeywordsList,
          specialGuidelines: formData.specialGuidelines
        }
      };

      const response = await clientAPI.updateClient(id, payload);
      
      if (response.success) {
        setSuccess(true);
        setTimeout(() => {
          navigate(`/clients/${id}`);
        }, 2000);
      } else {
        setErrors({ submit: response.message || 'Failed to update client' });
      }
    } catch (error) {
      setErrors({ submit: error.response?.data?.message || 'An error occurred' });
    } finally {
      setLoading(false);
    }
  };

  // Note: This is a simplified EditClient. For full form, copy ClientForm.jsx and modify handleSubmit to use updateClient
  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Edit Client</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <a href="#dashboard">Dashboard</a>
                </li>
                <li className="breadcrumb-item">
                  <a href="#clients">Clients</a>
                </li>
                <li className="breadcrumb-item active">Edit</li>
              </ol>
            </div>
          </div>
        </div>
      </div>

      {success && (
        <div className="alert alert-success alert-dismissible fade show" role="alert">
          Client updated successfully! Redirecting...
          <button type="button" className="btn-close" onClick={() => setSuccess(false)}></button>
        </div>
      )}

      {errors.submit && (
        <div className="alert alert-danger alert-dismissible fade show" role="alert">
          {errors.submit}
          <button type="button" className="btn-close" onClick={() => setErrors({ ...errors, submit: '' })}></button>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* Basic Client Information */}
        <div className="card mb-4">
          <div className="card-header">
            <h5 className="mb-0">Client Information</h5>
          </div>
          <div className="card-body">
            <div className="row g-3">
              <div className="col-md-6">
                <label htmlFor="companyName" className="form-label">Company Name:</label>
                <input
                  type="text"
                  className="form-control"
                  id="companyName"
                  name="companyName"
                  value={formData.companyName}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="col-md-6">
                <label htmlFor="contactPerson" className="form-label">Contact Person:</label>
                <input
                  type="text"
                  className="form-control"
                  id="contactPerson"
                  name="contactPerson"
                  value={formData.contactPerson}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="col-md-6">
                <label htmlFor="phone" className="form-label">Phone:</label>
                <input
                  type="tel"
                  className="form-control"
                  id="phone"
                  name="phone"
                  value={formData.phone}
                  onChange={handleChange}
                />
              </div>
              <div className="col-md-6">
                <label htmlFor="email" className="form-label">Email:</label>
                <input
                  type="email"
                  className="form-control"
                  id="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                />
              </div>
            </div>
          </div>
        </div>

        {/* WhatsApp & Email Management Section - Only for Sales Person, Sales Manager, Owner, Admin */}
        {canManageNotifications && (
          <div className="card mb-4">
            <div className="card-header bg-info text-white">
              <h5 className="mb-0">
                <i className="fas fa-bell me-2"></i>
                Notification Settings (WhatsApp & Email)
              </h5>
            </div>
            <div className="card-body">
              <div className="alert alert-info mb-3">
                <i className="fas fa-info-circle me-2"></i>
                Configure how enquiries are notified to this client via Email and WhatsApp.
              </div>

              <div className="row g-3">
                {/* Email Settings */}
                <div className="col-12">
                  <h6 className="border-bottom pb-2 mb-3">
                    <i className="fas fa-envelope me-2 text-primary"></i>
                    Email Settings
                  </h6>
                </div>

                <div className="col-md-12">
                  <div className="form-check mb-3">
                    <input
                      className="form-check-input"
                      type="checkbox"
                      id="useSameEmailForEnquiries"
                      name="useSameEmailForEnquiries"
                      checked={formData.useSameEmailForEnquiries}
                      onChange={handleChange}
                    />
                    <label className="form-check-label" htmlFor="useSameEmailForEnquiries">
                      <strong>Use same email for enquiries</strong>
                      <br />
                      <small className="text-muted">If checked, enquiry notifications will be sent to the main email address above.</small>
                    </label>
                  </div>
                </div>

                {!formData.useSameEmailForEnquiries && (
                  <div className="col-md-12">
                    <label htmlFor="enquiryEmail" className="form-label">
                      Enquiry Email Address <span className="text-danger">*</span>
                    </label>
                    <input
                      type="email"
                      className="form-control"
                      id="enquiryEmail"
                      name="enquiryEmail"
                      value={formData.enquiryEmail}
                      onChange={handleChange}
                      placeholder="enquiries@company.com"
                      required={!formData.useSameEmailForEnquiries}
                    />
                    <small className="form-text text-muted">
                      Email address where enquiry notifications will be sent (if different from main email)
                    </small>
                  </div>
                )}

                {/* WhatsApp Settings */}
                <div className="col-12 mt-4">
                  <h6 className="border-bottom pb-2 mb-3">
                    <i className="fab fa-whatsapp me-2 text-success"></i>
                    WhatsApp Settings
                  </h6>
                </div>

                <div className="col-md-12">
                  <div className="form-check mb-3">
                    <input
                      className="form-check-input"
                      type="checkbox"
                      id="useWhatsAppService"
                      name="useWhatsAppService"
                      checked={formData.useWhatsAppService}
                      onChange={handleChange}
                    />
                    <label className="form-check-label" htmlFor="useWhatsAppService">
                      <strong>Enable WhatsApp notifications</strong>
                      <br />
                      <small className="text-muted">If enabled, enquiry notifications will be sent via WhatsApp to the client.</small>
                    </label>
                  </div>
                </div>

                {formData.useWhatsAppService && (
                  <>
                    <div className="col-md-12">
                      <div className="form-check mb-3">
                        <input
                          className="form-check-input"
                          type="checkbox"
                          id="whatsAppSameAsMobile"
                          name="whatsAppSameAsMobile"
                          checked={formData.whatsAppSameAsMobile}
                          onChange={handleChange}
                        />
                        <label className="form-check-label" htmlFor="whatsAppSameAsMobile">
                          <strong>WhatsApp number same as mobile number</strong>
                          <br />
                          <small className="text-muted">If checked, WhatsApp notifications will be sent to the contact number above.</small>
                        </label>
                      </div>
                    </div>

                    {!formData.whatsAppSameAsMobile && (
                      <div className="col-md-12">
                        <label htmlFor="whatsAppNumber" className="form-label">
                          WhatsApp Number <span className="text-danger">*</span>
                        </label>
                        <input
                          type="tel"
                          className="form-control"
                          id="whatsAppNumber"
                          name="whatsAppNumber"
                          value={formData.whatsAppNumber}
                          onChange={handleChange}
                          placeholder="+91-9876543210"
                          required={!formData.whatsAppSameAsMobile && formData.useWhatsAppService}
                        />
                        <small className="form-text text-muted">
                          WhatsApp number where enquiry notifications will be sent (include country code, e.g., +91)
                        </small>
                      </div>
                    )}
                  </>
                )}

                <div className="col-12 mt-3">
                  <div className="alert alert-warning">
                    <i className="fas fa-exclamation-triangle me-2"></i>
                    <strong>Note:</strong> Email notifications are sent by default. WhatsApp notifications are optional and only sent if enabled above.
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Submit Button */}
        <div className="d-flex justify-content-end gap-2 mb-4">
          <button type="button" className="btn btn-secondary" onClick={() => navigate(-1)}>
            Cancel
          </button>
          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? 'Updating...' : 'Update Client'}
          </button>
        </div>
      </form>
    </Layout>
  );
};

export default EditClient;

