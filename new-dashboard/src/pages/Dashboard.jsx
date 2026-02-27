import Layout from '../components/Layout';

const Dashboard = () => {
  return (
    <Layout>
      {/* Page Title */}
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Dashboard</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <a href="#dashboard">Dashboard</a>
                </li>
                <li className="breadcrumb-item active">Main</li>
              </ol>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content - Blank for now */}
      <div className="row">
        <div className="col-12">
          <div className="card">
            <div className="card-body">
              <h5 className="card-title mb-4">Welcome to One Rank Digital Dashboard</h5>
              <p className="text-muted">Dashboard content will be added here...</p>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default Dashboard;
