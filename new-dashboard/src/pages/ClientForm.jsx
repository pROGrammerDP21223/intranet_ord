import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import { clientAPI, serviceAPI, imageUploadAPI } from '../services/api';
import { useRole } from '../hooks/useRole';
import { showToast } from '../utils/toast';
import { useLoading } from '../contexts/LoadingContext';

const ClientForm = () => {
  const role = useRole();
  const { startLoading, stopLoading } = useLoading();
  const [services, setServices] = useState([]);
  const [loadingServices, setLoadingServices] = useState(true);
  
  // Check if user can manage WhatsApp/Email settings
  const canManageNotifications = role.isAdmin || role.isOwner || role.isSalesPerson || role.isSalesManager;
  
  const [formData, setFormData] = useState({
    // Form Details
    formDate: new Date().toISOString().split('T')[0],
    amountWithoutGst: '50000',
    includeGst: true, // Checkbox to include/exclude GST
    gstPercentage: '18', // Default GST percentage
    gstAmount: '',
    totalPackage: '',
    
    // Client Information
    companyName: 'ABC Technologies Pvt. Ltd.',
    contactPerson: 'John Doe',
    designation: 'CEO',
    address: '123 Business Street, Sector 5, Mumbai - 400001',
    phone: '+91-9876543210',
    email: 'john.doe@abctech.com',
    domainName: 'www.abctech.com',
    companyLogo: '',
    gstNo: '27AABCU9603R1ZX',
    
    // WhatsApp & Email Management (only for Sales Person, Sales Manager, Owner, Admin)
    whatsAppNumber: '',
    enquiryEmail: '',
    useWhatsAppService: false,
    whatsAppSameAsMobile: true,
    useSameEmailForEnquiries: true,
    
    // Services (using service IDs)
    serviceIds: [],
    
    // Email Services
    emailServices: [],
    popIdCount: '',
    gSuiteIdCount: '',
    
    // Guidelines
    specificGuidelines: 'Please ensure the website is mobile responsive and follows modern design principles. SEO should focus on local keywords for Mumbai market.',
    
    // SEO Details
    seoKeywordRange: '25-50',
    seoLocation: 'Mumbai, Maharashtra',
    seoKeywordsList: '1. Digital Marketing Services\n2. Web Development Mumbai\n3. SEO Services India\n4. E-commerce Solutions\n5. Mobile App Development',
    
    // AdWords Details
    adwordsKeywords: '50',
    adwordsPeriod: 'Monthly',
    adwordsLocation: 'Mumbai, Pune, Delhi',
    adwordsKeywordsList: 'Digital Marketing\nWeb Development\nSEO Services\nE-commerce\nMobile Apps',
    specialGuidelines: 'Focus on B2B clients. Budget allocation: 60% for search ads, 40% for display ads.',
  });

  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [companyLogoFile, setCompanyLogoFile] = useState(null);
  const [companyLogoPreview, setCompanyLogoPreview] = useState('');

  // Load services from backend
  useEffect(() => {
    loadServices();
  }, []);

  const loadServices = async () => {
    try {
      setLoadingServices(true);
      const response = await serviceAPI.getServices();
      if (response.success && response.data) {
        setServices(response.data);
      }
    } catch (error) {
      console.error('Failed to load services:', error);
    } finally {
      setLoadingServices(false);
    }
  };

  // Calculate GST Amount and Total Package when AmountWithoutGst, GstPercentage, or includeGst changes
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
          // If checked, clear WhatsApp number as it will use phone
          whatsAppNumber: checked ? '' : prev.whatsAppNumber
        }));
      } else if (name === 'useSameEmailForEnquiries') {
        setFormData(prev => ({
          ...prev,
          useSameEmailForEnquiries: checked,
          // If checked, clear enquiry email as it will use main email
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
    
    // Clear error for this field
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };


  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.companyName.trim()) {
      newErrors.companyName = 'Company Name is required';
    }
    if (!formData.contactPerson.trim()) {
      newErrors.contactPerson = 'Contact Person is required';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    setSuccess(false);
    startLoading('Creating client...');

    try {
      let companyLogoUrl = formData.companyLogo;
      if (companyLogoFile) {
        const uploadResponse = await imageUploadAPI.uploadImage(companyLogoFile, 'clients');
        if (uploadResponse.success && uploadResponse.data) {
          companyLogoUrl = uploadResponse.data.url;
        } else {
          setErrors({ submit: 'Failed to upload company logo' });
          showToast.error('Failed to upload company logo');
          return;
        }
      }

      const payload = {
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
        companyLogo: companyLogoUrl,
        gstNo: formData.gstNo,
        specificGuidelines: formData.specificGuidelines,
        services: formData.serviceIds.map(serviceId => ({
          serviceId
        })),
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

      const response = await clientAPI.createClient(payload);
      
      if (response.success) {
        setSuccess(true);
        showToast.success('Client created successfully!');
        // Reset form after 2 seconds
        setTimeout(() => {
          window.location.reload();
        }, 2000);
      } else {
        const errorMsg = response.message || 'Failed to create client';
        setErrors({ submit: errorMsg });
        showToast.error(errorMsg);
      }
    } catch (error) {
      const errorMsg = error.response?.data?.message || 'An error occurred';
      setErrors({ submit: errorMsg });
      showToast.error(errorMsg);
    } finally {
      setLoading(false);
      stopLoading();
    }
  };

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Client Form</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <a href="#dashboard">Dashboard</a>
                </li>
                <li className="breadcrumb-item active">Client Form</li>
              </ol>
            </div>
          </div>
        </div>
      </div>

      {success && (
        <div className="alert alert-success alert-dismissible fade show" role="alert">
          Client form submitted successfully!
          <button type="button" className="btn-close" onClick={() => setSuccess(false)}></button>
        </div>
      )}

      {errors.submit && (
        <div className="alert alert-danger alert-dismissible fade show" role="alert">
          {errors.submit}
          <button type="button" className="btn-close" onClick={() => setErrors({ ...errors, submit: '' })}></button>
        </div>
      )}

      <form id="clientForm" onSubmit={handleSubmit}>
        <fieldset disabled={loading}>
        {/* Header Section */}
        <div className="card mb-4">
          <div className="card-body">
            <div className="row">
              <div className="col-md-6">
                <div className="mb-3">
                  <img 
                    src="https://ordbusinesshub.com/images/logo.png"
                    alt="OneRank Digital Logo" 
                    className="img-fluid"
                    style={{ maxHeight: '80px' }}
                  />
                </div>
                <address className="mb-0">
                  <div className="mb-2">
                    <i className="fas fa-envelope me-2 text-primary"></i>
                    <a href="mailto:support@onerankdigital.com">support@onerankdigital.com</a>
                  </div>
                  <div className="mb-2">
                    <i className="fas fa-phone me-2 text-primary"></i>
                    <a href="tel:8007231573">80072 31573</a> / <a href="tel:7506555044">75065 55044</a>
                  </div>
                  <div className="mb-2">
                    <i className="fas fa-map-marker-alt me-2 text-primary"></i>
                    <span>Ram Tekdi Path, Ram Tekdi, Sewree, Mumbai - 400015</span>
                  </div>
                  <div>
                    <i className="fas fa-map-marker-alt me-2 text-primary"></i>
                    <span>Pune: Megapolis, Hinjewadi Phase-3, Pune - 411057</span>
                  </div>
                </address>
              </div>
              <div className="col-md-6">
                <h1 className="h3 mb-2">Client Form</h1>
                <p className="text-muted mb-3">Trusted Digital Marketing Company In India</p>
                
                <div className="row g-3">
                  <div className="col-md-4">
                    <label htmlFor="formDate" className="form-label">Date:</label>
                    <input
                      type="date"
                      className="form-control"
                      id="formDate"
                      name="formDate"
                      value={formData.formDate}
                      onChange={handleChange}
                      required
                    />
                  </div>
                  <div className="col-md-4">
                    <label htmlFor="totalPackage" className="form-label">Total Package:</label>
                    <input
                      type="text"
                      className="form-control"
                      id="totalPackage"
                      name="totalPackage"
                      value={formData.totalPackage}
                      readOnly
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Client Information */}
        <div className="card mb-4">
          <div className="card-header">
            <h5 className="mb-0">Client Information</h5>
          </div>
          <div className="card-body">
            <div className="row g-3">
              <div className="col-md-12">
                <label htmlFor="companyName" className="form-label">Company Name:</label>
                <input
                  type="text"
                  className={`form-control ${errors.companyName ? 'is-invalid' : ''}`}
                  id="companyName"
                  name="companyName"
                  value={formData.companyName}
                  onChange={handleChange}
                  required
                />
                {errors.companyName && <div className="invalid-feedback">{errors.companyName}</div>}
              </div>
              
              <div className="col-md-6">
                <label htmlFor="contactPerson" className="form-label">Contact Person:</label>
                <input
                  type="text"
                  className={`form-control ${errors.contactPerson ? 'is-invalid' : ''}`}
                  id="contactPerson"
                  name="contactPerson"
                  value={formData.contactPerson}
                  onChange={handleChange}
                  required
                />
                {errors.contactPerson && <div className="invalid-feedback">{errors.contactPerson}</div>}
              </div>
              
              <div className="col-md-6">
                <label htmlFor="designation" className="form-label">Designation:</label>
                <input
                  type="text"
                  className="form-control"
                  id="designation"
                  name="designation"
                  value={formData.designation}
                  onChange={handleChange}
                />
              </div>
              
              <div className="col-md-12">
                <label htmlFor="address" className="form-label">Address:</label>
                <textarea
                  className="form-control"
                  id="address"
                  name="address"
                  rows="3"
                  value={formData.address}
                  onChange={handleChange}
                ></textarea>
              </div>
              
              <div className="col-md-6">
                <label htmlFor="phone" className="form-label">Contact No.:</label>
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
                <label htmlFor="email" className="form-label">E-mail:</label>
                <input
                  type="email"
                  className="form-control"
                  id="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                />
              </div>
              
              <div className="col-md-6">
                <label htmlFor="domainName" className="form-label">Domain Name:</label>
                <input
                  type="text"
                  className="form-control"
                  id="domainName"
                  name="domainName"
                  value={formData.domainName}
                  onChange={handleChange}
                />
              </div>

              <div className="col-md-6">
                <label htmlFor="companyLogo" className="form-label">Company Logo/Image (Optional):</label>
                <input
                  type="file"
                  className="form-control"
                  id="companyLogo"
                  accept="image/*"
                  onChange={(e) => {
                    const file = e.target.files?.[0];
                    setCompanyLogoFile(file || null);
                    if (file) {
                      const reader = new FileReader();
                      reader.onloadend = () => setCompanyLogoPreview(reader.result);
                      reader.readAsDataURL(file);
                    } else {
                      setCompanyLogoPreview('');
                    }
                  }}
                />
                {(companyLogoPreview || formData.companyLogo) && (
                  <div className="mt-2">
                    <img
                      src={companyLogoPreview || formData.companyLogo}
                      alt="Company logo preview"
                      style={{ width: '120px', height: '120px', objectFit: 'cover', borderRadius: '6px' }}
                    />
                  </div>
                )}
              </div>
              
              <div className="col-md-6">
                <label htmlFor="gstNo" className="form-label">GSTIN No.:</label>
                <input
                  type="text"
                  className="form-control"
                  id="gstNo"
                  name="gstNo"
                  value={formData.gstNo}
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

        {/* Package Amount */}
        <div className="card mb-4">
          <div className="card-header">
            <h5 className="mb-0">Package Amount</h5>
          </div>
          <div className="card-body">
            <div className="row g-3 mb-3">
              <div className="col-md-12">
                <div className="form-check">
                  <input
                    className="form-check-input"
                    type="checkbox"
                    id="includeGst"
                    name="includeGst"
                    checked={formData.includeGst}
                    onChange={handleChange}
                  />
                  <label className="form-check-label" htmlFor="includeGst">
                    Include GST
                  </label>
                </div>
              </div>
            </div>
            <div className="row g-3">
              <div className="col-md-4">
                <label htmlFor="amountWithoutGst" className="form-label">Amount {formData.includeGst ? 'Without' : ''} GST:</label>
                <input
                  type="number"
                  step="0.01"
                  className="form-control"
                  id="amountWithoutGst"
                  name="amountWithoutGst"
                  value={formData.amountWithoutGst}
                  onChange={handleChange}
                />
              </div>
              {formData.includeGst && (
                <>
                  <div className="col-md-4">
                    <label htmlFor="gstPercentage" className="form-label">GST Percentage (%):</label>
                    <input
                      type="number"
                      step="0.01"
                      className="form-control"
                      id="gstPercentage"
                      name="gstPercentage"
                      value={formData.gstPercentage}
                      onChange={handleChange}
                    />
                  </div>
                  <div className="col-md-4">
                    <label htmlFor="gstAmount" className="form-label">GST Amount:</label>
                    <input
                      type="text"
                      className="form-control bg-light"
                      id="gstAmount"
                      name="gstAmount"
                      value={formData.gstAmount}
                      readOnly
                    />
                  </div>
                </>
              )}
              <div className="col-md-4">
                <label htmlFor="totalPackage" className="form-label">Total Package:</label>
                <input
                  type="text"
                  className="form-control bg-light"
                  id="totalPackage"
                  name="totalPackage"
                  value={formData.totalPackage}
                  readOnly
                />
              </div>
            </div>
          </div>
        </div>

        {/* Services */}
        <div className="card mb-4">
          <div className="card-header">
            <h5 className="mb-0">Select Services</h5>
          </div>
          <div className="card-body">
            {loadingServices ? (
              <div className="text-center py-4">
                <div className="spinner-border text-primary" role="status">
                  <span className="visually-hidden">Loading services...</span>
                </div>
              </div>
            ) : (
              <div className="row">
                {/* Email Services (POP ID and G Suite ID) */}
                <div className="col-md-3">
                  <h6>Email Services</h6>
                  <div className="form-check">
                    <input
                      className="form-check-input"
                      type="checkbox"
                      id="pop-id"
                      name="emailServices[]"
                      value="pop-id"
                      checked={formData.emailServices.includes('pop-id')}
                      onChange={handleChange}
                    />
                    <label className="form-check-label" htmlFor="pop-id">
                      POP ID
                    </label>
                    {formData.emailServices.includes('pop-id') && (
                      <input
                        type="number"
                        className="form-control form-control-sm mt-1"
                        placeholder="Qty"
                        min="1"
                        name="popIdCount"
                        value={formData.popIdCount}
                        onChange={handleChange}
                      />
                    )}
                  </div>
                  <div className="form-check">
                    <input
                      className="form-check-input"
                      type="checkbox"
                      id="g-suite-id"
                      name="emailServices[]"
                      value="g-suite-id"
                      checked={formData.emailServices.includes('g-suite-id')}
                      onChange={handleChange}
                    />
                    <label className="form-check-label" htmlFor="g-suite-id">
                      G Suite ID
                    </label>
                    {formData.emailServices.includes('g-suite-id') && (
                      <input
                        type="number"
                        className="form-control form-control-sm mt-1"
                        placeholder="Qty"
                        min="1"
                        name="gSuiteIdCount"
                        value={formData.gSuiteIdCount}
                        onChange={handleChange}
                      />
                    )}
                  </div>
                </div>

                {/* Dynamic Services from Backend */}
                {(() => {
                  const groupedServices = services.reduce((acc, service) => {
                    const category = service.category || 'Other';
                    if (!acc[category]) {
                      acc[category] = [];
                    }
                    acc[category].push(service);
                    return acc;
                  }, {});
                  
                  return Object.entries(groupedServices).map(([category, categoryServices]) => (
                    <div key={category} className="col-md-3">
                      <h6>{category}</h6>
                      {categoryServices.map((service) => (
                        <div key={service.id} className="form-check">
                          <input
                            className="form-check-input"
                            type="checkbox"
                            id={`service-${service.id}`}
                            name="services[]"
                            value={service.id}
                            checked={formData.serviceIds.includes(service.id)}
                            onChange={handleChange}
                          />
                          <label className="form-check-label" htmlFor={`service-${service.id}`}>
                            {service.serviceName}
                          </label>
                        </div>
                      ))}
                    </div>
                  ));
                })()}
              </div>
            )}
          </div>
        </div>

        {/* Specific Guidelines */}
        <div className="card mb-4">
          <div className="card-header">
            <h5 className="mb-0">Specific Guidelines</h5>
          </div>
          <div className="card-body">
            <label htmlFor="specificGuidelines" className="form-label">Specific Guidelines (Website & SEO):</label>
            <textarea
              className="form-control"
              id="specificGuidelines"
              name="specificGuidelines"
              rows="5"
              value={formData.specificGuidelines}
              onChange={handleChange}
            ></textarea>
          </div>
        </div>

        {/* SEO Details */}
        <div className="card mb-4">
          <div className="card-header">
            <h5 className="mb-0">SEARCH ENGINE OPTIMIZATION (SEO)</h5>
          </div>
          <div className="card-body">
            <div className="row g-3 mb-3">
              <div className="col-md-6">
                <label className="form-label">Keywords:</label>
                <div className="d-flex gap-3">
                  <div className="form-check">
                    <input
                      className="form-check-input"
                      type="radio"
                      id="seo-upto-25"
                      name="seoKeywordRange"
                      value="upto-25"
                      checked={formData.seoKeywordRange === 'upto-25'}
                      onChange={handleChange}
                    />
                    <label className="form-check-label" htmlFor="seo-upto-25">Upto 25</label>
                  </div>
                  <div className="form-check">
                    <input
                      className="form-check-input"
                      type="radio"
                      id="seo-25-50"
                      name="seoKeywordRange"
                      value="25-50"
                      checked={formData.seoKeywordRange === '25-50'}
                      onChange={handleChange}
                    />
                    <label className="form-check-label" htmlFor="seo-25-50">25 – 50</label>
                  </div>
                  <div className="form-check">
                    <input
                      className="form-check-input"
                      type="radio"
                      id="seo-75-100"
                      name="seoKeywordRange"
                      value="75-100"
                      checked={formData.seoKeywordRange === '75-100'}
                      onChange={handleChange}
                    />
                    <label className="form-check-label" htmlFor="seo-75-100">75 – 100</label>
                  </div>
                </div>
              </div>
              <div className="col-md-6">
                <label htmlFor="seoLocation" className="form-label">Location:</label>
                <input
                  type="text"
                  className="form-control"
                  id="seoLocation"
                  name="seoLocation"
                  placeholder="e.g., State, India, Country, Global"
                  value={formData.seoLocation}
                  onChange={handleChange}
                />
              </div>
            </div>
            <div className="mb-3">
              <label htmlFor="seoKeywordsList" className="form-label">Top Products / Keywords:</label>
              <textarea
                className="form-control"
                id="seoKeywordsList"
                name="seoKeywordsList"
                rows="8"
                placeholder="1.&#10;2.&#10;3.&#10;4.&#10;5.&#10;..."
                value={formData.seoKeywordsList}
                onChange={handleChange}
              ></textarea>
            </div>
          </div>
        </div>

        {/* Google AdWords */}
        <div className="card mb-4">
          <div className="card-header">
            <h5 className="mb-0">GOOGLE ADWORDS</h5>
          </div>
          <div className="card-body">
            <div className="row g-3 mb-3">
              <div className="col-md-6">
                <label htmlFor="adwordsKeywords" className="form-label">Number of Keywords:</label>
                <input
                  type="text"
                  className="form-control"
                  id="adwordsKeywords"
                  name="adwordsKeywords"
                  value={formData.adwordsKeywords}
                  onChange={handleChange}
                />
              </div>
              <div className="col-md-6">
                <label htmlFor="adwordsPeriod" className="form-label">Period:</label>
                <input
                  type="text"
                  className="form-control"
                  id="adwordsPeriod"
                  name="adwordsPeriod"
                  placeholder="(Monthly / Quarterly)"
                  value={formData.adwordsPeriod}
                  onChange={handleChange}
                />
              </div>
              <div className="col-md-12">
                <label htmlFor="adwordsLocation" className="form-label">Location:</label>
                <input
                  type="text"
                  className="form-control"
                  id="adwordsLocation"
                  name="adwordsLocation"
                  value={formData.adwordsLocation}
                  onChange={handleChange}
                />
              </div>
              <div className="col-md-12">
                <label htmlFor="adwordsKeywordsList" className="form-label">Keywords:</label>
                <textarea
                  className="form-control"
                  id="adwordsKeywordsList"
                  name="adwordsKeywordsList"
                  rows="4"
                  value={formData.adwordsKeywordsList}
                  onChange={handleChange}
                ></textarea>
              </div>
            </div>
          </div>
        </div>

        {/* Special Guidelines */}
        <div className="card mb-4">
          <div className="card-header">
            <h5 className="mb-0">SPECIAL GUIDELINES</h5>
          </div>
          <div className="card-body">
            <textarea
              className="form-control"
              id="specialGuidelines"
              name="specialGuidelines"
              rows="5"
              value={formData.specialGuidelines}
              onChange={handleChange}
            ></textarea>
          </div>
        </div>

        {/* Submit Button */}
        <div className="d-flex justify-content-end gap-2 mb-4">
          <button type="button" className="btn btn-secondary" onClick={() => window.location.reload()} disabled={loading}>
            Reset
          </button>
          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? (
              <>
                <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                Submitting...
              </>
            ) : (
              'Submit Form'
            )}
          </button>
        </div>
        </fieldset>
      </form>
    </Layout>
  );
};

export default ClientForm;

