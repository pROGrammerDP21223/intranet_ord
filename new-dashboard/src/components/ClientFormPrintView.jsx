import './ClientFormPrint.css';

/**
 * Client Form Print View - Mirrors print.html exactly, fills placeholders with client data.
 */
const ClientFormPrintView = ({ client }) => {
  if (!client) return null;

  const formatDate = (dateString) => {
    if (!dateString) return '';
    return new Date(dateString).toLocaleDateString('en-GB');
  };

  const clientServices     = client.clientServices      || [];
  const clientEmailServices = client.clientEmailServices || [];
  const seoDetail          = client.clientSeoDetail      || {};
  const adwordsDetail      = client.clientAdwordsDetail  || {};

  /** Check if a service (by print.html value) is selected */
  const hasService = (value) => {
    const words = value.replace(/-/g, ' ').toLowerCase();
    return clientServices.some(cs => {
      const name = (cs.service?.serviceName || '').toLowerCase();
      return (
        name === words ||
        name.replace(/[\s/&]+/g, '-').replace(/-+/g, '-') === value ||
        name.includes(words) ||
        words.includes(name)
      );
    });
  };

  /** Check if an email service is selected */
  const hasEmailService = (value) => {
    const words = value.replace(/-/g, ' ').toLowerCase();
    return clientEmailServices.some(es => {
      const type = (es.emailServiceType || '').toLowerCase();
      return type === words || type.replace(/\s+/g, '-') === value;
    });
  };

  const getEmailServiceQty = (value) => {
    const words = value.replace(/-/g, ' ').toLowerCase();
    const es = clientEmailServices.find(es => {
      const type = (es.emailServiceType || '').toLowerCase();
      return type === words || type.replace(/\s+/g, '-') === value;
    });
    return es?.quantity != null ? String(es.quantity) : '';
  };

  const seoRanges    = (seoDetail.keywordRange || '').split(',').map(s => s.trim().toLowerCase());
  const seoLocations = (seoDetail.location     || '').split(',').map(s => s.trim().toLowerCase());
  const hasSeoRange    = (v) => seoRanges.includes(v.toLowerCase());
  const hasSeoLocation = (v) => seoLocations.includes(v.toLowerCase());

  return (
    <>
      <div className="container">
        <main className="form-page">

          {/* ── HEADER ── */}
          <header className="form-header">
            <div className="header-left">
              <div className="logo-section">
                <img
                  src="https://onerankdigital.com/wp-content/uploads/2023/10/FINAL-1024x347.png"
                  alt="OneRank Digital Logo"
                  className="company-logo"
                />
              </div>
              <address className="contact-info">
                <div className="contact-item">
                  <span className="icon" aria-hidden="true">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="#e91e63" viewBox="0 0 24 24">
                      <path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.89 2 1.99 2H20c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/>
                    </svg>
                  </span>
                  <a href="mailto:support@onerankdigital.com">support@onerankdigital.com</a>
                </div>
                <div className="contact-item">
                  <span className="icon" aria-hidden="true">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="#e91e63" viewBox="0 0 24 24">
                      <path d="M6.62 10.79a15.054 15.054 0 006.59 6.59l2.2-2.2a1 1 0 011.11-.21c1.21.49 2.53.76 3.88.76a1 1 0 011 1v3.5a1 1 0 01-1 1C10.07 22 2 13.93 2 4a1 1 0 011-1H6.5a1 1 0 011 1c0 1.35.27 2.67.76 3.88a1 1 0 01-.21 1.11l-2.2 2.2z"/>
                    </svg>
                  </span>
                  <a href="tel:8007231573">80072 31573</a> / <a href="tel:7506555044">75065 55044</a>
                </div>
                <div className="contact-item">
                  <span className="icon" aria-hidden="true">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="#e91e63" viewBox="0 0 24 24">
                      <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5a2.5 2.5 0 110-5 2.5 2.5 0 010 5z"/>
                    </svg>
                  </span>
                  <span className="text-dark">Ram Tekdi Path, Ram Tekdi, Sewree, Mumbai - 400015</span>
                </div>
                <div className="contact-item">
                  <span className="icon" aria-hidden="true">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="#e91e63" viewBox="0 0 24 24">
                      <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5a2.5 2.5 0 110-5 2.5 2.5 0 010 5z"/>
                    </svg>
                  </span>
                  <span className="text-dark">Pune: Megapolis, Hinjewadi Phase-3, Pune - 411057</span>
                </div>
              </address>
            </div>

            <div className="header-right">
              <h1 className="form-title">Order Form</h1>
              <p className="form-subtitle">Trusted Digital Marketing Company In India</p>
              <div className="form-details-box">
                <div className="form-detail-item">
                  <label htmlFor="formNo">Customer No:</label>
                  <input type="text" id="formNo" value={client.customerNo || ''} readOnly />
                </div>
                <div className="form-detail-item">
                  <label htmlFor="formDate">Date:</label>
                  <input type="text" id="formDate" value={formatDate(client.formDate || client.createdAt)} readOnly />
                </div>
                <div className="form-detail-item">
                  <label htmlFor="totalPackage">Total Package:</label>
                  <input
                    type="text"
                    id="totalPackage"
                    value={client.totalPackage ? `₹${Number(client.totalPackage).toLocaleString('en-IN')}` : ''}
                    readOnly
                  />
                </div>
              </div>
            </div>
          </header>

          {/* ── MAIN FORM ── */}
          <form id="orderForm" className="main-form">

            {/* CLIENT INFORMATION */}
            <section className="form-section">
              <h2 className="section-heading">Client Information</h2>
              <div className="form-grid">
                <div className="form-group full-width">
                  <label htmlFor="companyName">Company Name:</label>
                  <input type="text" id="companyName" value={client.companyName || ''} readOnly />
                </div>
                <div className="form-row">
                  <div className="form-group half-width">
                    <label htmlFor="fullName">Contact Person:</label>
                    <input type="text" id="fullName" value={client.contactPerson || ''} readOnly />
                  </div>
                  <div className="form-group half-width">
                    <label htmlFor="designation">Designation:</label>
                    <input type="text" id="designation" value={client.designation || ''} readOnly />
                  </div>
                </div>
                <div className="form-group full-width">
                  <label htmlFor="address">Address:</label>
                  <textarea id="address" rows="3" value={client.address || ''} readOnly />
                </div>
                <div className="form-row">
                  <div className="form-group half-width">
                    <label htmlFor="phone">Contact No.:</label>
                    <input type="tel" id="phone" value={client.phone || ''} readOnly />
                  </div>
                  <div className="form-group half-width">
                    <label htmlFor="email">E-mail:</label>
                    <input type="email" id="email" value={client.email || ''} readOnly />
                  </div>
                </div>
                <div className="form-row">
                  <div className="form-group half-width">
                    <label htmlFor="domainName">Domain Name:</label>
                    <input type="text" id="domainName" value={client.domainName || ''} readOnly />
                  </div>
                  <div className="form-group half-width">
                    <label htmlFor="gstNo">GSTIN No.:</label>
                    <input type="text" id="gstNo" value={client.gstNo || ''} readOnly />
                  </div>
                </div>
              </div>
            </section>

            {/* SELECT SERVICES */}
            <section className="form-section">
              <h2 className="section-heading">Select Services</h2>
              <div className="services-grid">

                {/* Domain & Hosting */}
                <div className="service-column">
                  <div className="service-category">Domain &amp; Hosting</div>
                  <fieldset className="checkbox-list">
                    <legend className="sr-only">Domain &amp; Hosting</legend>
                    <label className="checkbox-item">
                      <input type="checkbox" checked={hasService('domain-hosting')} onChange={() => {}} />
                      <span>Domain &amp; Hosting</span>
                    </label>
                    <label className="checkbox-item">
                      <input type="checkbox" id="popId" checked={hasEmailService('pop-id')} onChange={() => {}} />
                      <span>POP ID</span>
                      <input type="number" id="popIdCount" value={getEmailServiceQty('pop-id')} onChange={() => {}} disabled={!hasEmailService('pop-id')} placeholder="Qty" />
                    </label>
                    <label className="checkbox-item">
                      <input type="checkbox" id="gSuiteId" checked={hasEmailService('g-suite-id')} onChange={() => {}} />
                      <span>G Suite ID</span>
                      <input type="number" id="gSuiteIdCount" value={getEmailServiceQty('g-suite-id')} onChange={() => {}} disabled={!hasEmailService('g-suite-id')} placeholder="Qty" />
                    </label>
                  </fieldset>
                </div>

                {/* Web Design */}
                <div className="service-column">
                  <div className="service-category">Web Design</div>
                  <fieldset className="checkbox-list">
                    <legend className="sr-only">Web Design</legend>
                    <label className="checkbox-item">
                      <input type="checkbox" checked={hasService('website-design-development')} onChange={() => {}} />
                      <span>Website Design / Development</span>
                    </label>
                    <label className="checkbox-item">
                      <input type="checkbox" checked={hasService('website-maintenance')} onChange={() => {}} />
                      <span>Website Maintenance</span>
                    </label>
                    <label className="checkbox-item">
                      <input type="checkbox" checked={hasService('app-development')} onChange={() => {}} />
                      <span>App Development</span>
                    </label>
                  </fieldset>
                </div>

                {/* SEO & Ads */}
                <div className="service-column">
                  <div className="service-category">SEO &amp; Ads</div>
                  <fieldset className="checkbox-list">
                    <legend className="sr-only">SEO</legend>
                    <label className="checkbox-item">
                      <input type="checkbox" checked={hasService('seo')} onChange={() => {}} />
                      <span>Search Engine Optimization</span>
                    </label>
                    <label className="checkbox-item">
                      <input type="checkbox" checked={hasService('google-ads')} onChange={() => {}} />
                      <span>Google Ads / PPC</span>
                    </label>
                    <label className="checkbox-item">
                      <input type="checkbox" checked={hasService('google-my-business')} onChange={() => {}} />
                      <span>Google My Business (Local)</span>
                    </label>
                  </fieldset>
                </div>

                {/* Digital Marketing */}
                <div className="service-column">
                  <div className="service-category">Digital Marketing</div>
                  <fieldset className="checkbox-list">
                    <legend className="sr-only">Additional Services</legend>
                    <label className="checkbox-item">
                      <input type="checkbox" checked={hasService('ai-chatbot')} onChange={() => {}} />
                      <span>AI Chatbot</span>
                    </label>
                    <label className="checkbox-item">
                      <input type="checkbox" checked={hasService('youtube-promotion')} onChange={() => {}} />
                      <span>YouTube Promotion</span>
                    </label>
                    <label className="checkbox-item">
                      <input type="checkbox" checked={hasService('email-marketing')} onChange={() => {}} />
                      <span>Email Marketing</span>
                    </label>
                  </fieldset>
                </div>

              </div>
            </section>

            {/* SPECIFIC GUIDELINES */}
            <section className="form-section">
              <h2 className="section-heading">Specific Guidelines</h2>
              <div className="form-group full-width" style={{ padding: '20px' }}>
                <label htmlFor="guidelines">Website &amp; SEO Guidelines:</label>
                <textarea id="guidelines" rows="5" value={client.specificGuidelines || ''} readOnly />
              </div>
            </section>

            {/* SERVICE-SPECIFIC DETAILS */}
            <div className="special-instructions">
              <h2 className="instructions-title">Service-Specific Details</h2>

              {/* SEO */}
              <fieldset className="instructions-section">
                <legend className="instruction-category">SEARCH ENGINE OPTIMIZATION (SEO)</legend>
                <div className="form-group inline-group">
                  <label>Keywords:</label>
                  <label className="checkbox-item">
                    <input type="checkbox" checked={hasSeoRange('upto-25')} onChange={() => {}} />
                    Upto 25
                  </label>
                  <label className="checkbox-item">
                    <input type="checkbox" checked={hasSeoRange('25-50')} onChange={() => {}} />
                    25 – 50
                  </label>
                  <label className="checkbox-item">
                    <input type="checkbox" checked={hasSeoRange('75-100')} onChange={() => {}} />
                    75 – 100
                  </label>
                </div>
                <div className="form-group inline-group">
                  <label>Location:</label>
                  <label className="checkbox-item">
                    <input type="checkbox" checked={hasSeoLocation('state-wise')} onChange={() => {}} />
                    State
                  </label>
                  <label className="checkbox-item">
                    <input type="checkbox" checked={hasSeoLocation('india')} onChange={() => {}} />
                    India
                  </label>
                  <label className="checkbox-item">
                    <input type="checkbox" checked={hasSeoLocation('country-wise')} onChange={() => {}} />
                    Country
                  </label>
                  <label className="checkbox-item">
                    <input type="checkbox" checked={hasSeoLocation('global')} onChange={() => {}} />
                    Global
                  </label>
                </div>
                <div className="form-group">
                  <label>Top Products / Keywords:</label>
                  <textarea rows="8" value={seoDetail.keywordsList || ''} readOnly placeholder="1.&#10;2.&#10;3." />
                </div>
              </fieldset>

              {/* GOOGLE ADWORDS */}
              <fieldset className="instructions-section">
                <legend className="instruction-category">GOOGLE ADWORDS</legend>
                <div className="form-group">
                  <label>Number of Keywords:</label>
                  <input type="text" value={adwordsDetail.numberOfKeywords || ''} readOnly />
                  <input type="text" value={adwordsDetail.period || ''} readOnly placeholder="(Monthly / Quarterly)" />
                </div>
                <div className="form-group">
                  <label>Location:</label>
                  <input type="text" value={adwordsDetail.location || ''} readOnly />
                </div>
                <div className="form-group">
                  <label>Keywords:</label>
                  <textarea rows="4" value={adwordsDetail.keywordsList || ''} readOnly />
                </div>
              </fieldset>

              {/* SPECIAL GUIDELINES */}
              <fieldset className="instructions-section">
                <legend className="instruction-category">SPECIAL GUIDELINES</legend>
                <div className="form-group">
                  <textarea rows="5" value={adwordsDetail.specialGuidelines || ''} readOnly />
                </div>
              </fieldset>
            </div>

          </form>

          {/* ── TERMS & CONDITIONS ── */}
          <section className="terms-section">
            <h2 className="terms-title">Terms &amp; Conditions</h2>
            <ol className="terms-list">
              <li><strong>The information contained on the website is solely the responsibility of the client's signatory, and the signatory hereby agrees to fully indemnify One Rank Digital in this regard as and when any claims, demands, etc.</strong></li>
              <li><strong>The website will be placed on the signatory's domain name within one month of receiving the complete website matter, photos, designs, and so on, as well as receipt of the full payment.</strong></li>
              <li><strong>Please make sure that the website content, product information, and photos you provide are your own.</strong></li>
              <li><strong>If a legal dispute arises in the future, our company will not be held liable under any circumstances.</strong></li>
              <li><strong>If you do not renew your web package after one year, we will not be responsible for domain names, website content, or mailing services.</strong></li>
              <li><strong>Any applicable government taxes or levies will be assessed.</strong></li>
              <li><strong>Scope of Work:</strong> One Rank Digital agrees to provide website design and promotion services as detailed in the project proposal or agreement.</li>
              <li><strong>Client Responsibilities:</strong> The client agrees to provide all necessary materials, information, and approvals required for the timely completion of the project. Timely communication and feedback from the client are essential for the progress of the project.</li>
              <li><strong>Payment Terms:</strong> The client agrees to pay 60% of the total project cost as an upfront deposit before the commencement of work. Milestone payments will be specified in the project proposal, with the final payment due upon project completion. Late payments may incur additional charges, and the Company reserves the right to suspend work until outstanding payments are received.</li>
              <li><strong>Intellectual Property:</strong> Upon full payment, the client will own the intellectual property rights to the final website design. The Company retains the right to use non-proprietary elements for promotional purposes.</li>
              <li><strong>Timeline and Delays:</strong> A project timeline will be provided in the project proposal. Delays caused by the client may result in an extension of the project timeline.</li>
              <li><strong>Termination of Services:</strong> Either party may terminate the agreement in writing if the other party breaches its obligations. Termination fees may apply.</li>
              <li><strong>Confidentiality:</strong> Both parties agree to keep confidential information exchanged during the project confidential.</li>
              <li><strong>Hosting and Domain:</strong> If hosting and domain services are provided by the Company, the client agrees to adhere to the terms and conditions of the hosting provider.</li>
              <li><strong>Legal Compliance:</strong> The client is responsible for ensuring the website complies with applicable laws and regulations.</li>
              <li><strong>Updates to Terms and Conditions:</strong> The Company reserves the right to update these terms and conditions. Clients will be notified of any changes.</li>
            </ol>
          </section>

          {/* ── FOOTER (SIGNATURE) ── */}
          <footer className="form-footer">
            <div className="footer-left">
              <div className="footer-block">
                <label className="footer-label" htmlFor="nameDesignation">Name &amp; Designation:</label>
                <input type="text" id="nameDesignation" className="signature-input" value={client.nameDesignation || ''} readOnly placeholder="Enter Name &amp; Designation" />
              </div>
              <div className="footer-block">
                <label className="footer-label" htmlFor="signature">Signature:</label>
                <input type="text" id="signature" className="signature-input" value={client.signature || ''} readOnly placeholder="Enter Signature" />
              </div>
            </div>
            <div className="footer-right">
              <div className="footer-block">
                <label className="footer-label">E-Signature:</label>
                {client.eSignature ? (
                  <img
                    src={client.eSignature}
                    alt="E-Signature"
                    style={{ width: '100%', maxHeight: '100px', border: '2px solid #c5cae9', borderRadius: '6px', marginBottom: '8px' }}
                  />
                ) : (
                  <canvas id="esignCanvas" className="signature-canvas" style={{ pointerEvents: 'none', opacity: 0.4 }} />
                )}
              </div>
            </div>
          </footer>

          {/* ── BOTTOM FOOTER ── */}
          <div className="bottom-footer">
            <div className="footer-curve"></div>
            <div className="footer-content">
              <span className="globe-icon">🌐</span>
              <span className="website-url">www.onerankdigital.com</span>
            </div>
          </div>

        </main>
      </div>
    </>
  );
};

export default ClientFormPrintView;
