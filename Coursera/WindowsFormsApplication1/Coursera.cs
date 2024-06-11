using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using CrystalDecisions.Shared;

namespace WindowsFormsApplication1
{
    public partial class Registration_form : Form
    {

        OracleDataAdapter adapter;
        OracleCommandBuilder builder;
        DataSet ds;
        Courses_CrystalReport CCR;
        Students_CrystalReport SCR;
        string ordb = "Data source=orcl;User Id=hr; Password=hr;";
        OracleConnection conn;

        public Registration_form()
        {
            InitializeComponent();
            
        }

        private void signUp_lnklbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SignUp_pnl.BringToFront();
        }

        private void signIn_lnklbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SignIn_pnl.BringToFront();
        }

        private void signIn_btn_Click(object sender, EventArgs e)
        {
            string email = signIn_email_txtbox.Text;
            string password = signIn_password_txtbox.Text;

            // Check if any of the textboxes are empty
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || !signIn_gpbox.Controls.OfType<RadioButton>().Any(rb => rb.Checked))
            {
                MessageBox.Show("Please fill in all the fields.");
                return; // Stop further execution
            }

            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT UserId, Username, UserType, IsActive FROM Users WHERE Email = :Email AND Password = :Password";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("Email", email);
            cmd.Parameters.Add("Password", password);

            OracleDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                int userId = Convert.ToInt32(dr["UserId"]);
                string username = dr["Username"].ToString();
                string userType = dr["UserType"].ToString();
                string isActive = dr["IsActive"].ToString();

                if (isActive == "n")
                {
                    MessageBox.Show("Your account is not active. Please contact support for assistance.");
                    return;
                }
                else
                {
                    if (userType == "s" && signIn_student_rb.Checked)
                    {
                        MessageBox.Show("Welcome back, " + username);
                        User_page_pnl.BringToFront();
                    }
                    else if (userType == "i" && signIn_instructor_rb.Checked)
                    {
                        Instructor_page_pnl.BringToFront();
                    }
                    else if (userType == "a" && signIn_admin_rb.Checked)
                    {
                        Admin_page_pnl.BringToFront();
                    }
                    else
                        MessageBox.Show("This account is incompatible with the chosen user type", "Account Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Clear the sign-in form
                    signIn_email_txtbox.Text = "";
                    signIn_password_txtbox.Text = "";
                    // Clear selected radio buttons in the group
                    foreach (RadioButton rb in signIn_gpbox.Controls.OfType<RadioButton>())
                    {
                        rb.Checked = false;
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid email or password. Please try again.");
                // Clear the sign-in form
                signIn_email_txtbox.Text = "";
                signIn_password_txtbox.Text = "";
                // Clear selected radio buttons in the group
                foreach (RadioButton rb in signIn_gpbox.Controls.OfType<RadioButton>())
                {
                    rb.Checked = false;
                }
            }

            dr.Close();
            cmd.Dispose();
        }

        private void signUp_btn_Click(object sender, EventArgs e)
        {
            string username = signUp_username_txtbox.Text;
            string email = signUp_email_txtbox.Text;
            string password = signUp_password_txtbox.Text;
            //int newId;
            // Check if any of the textboxes are empty
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill in all the fields.");
                return; // Stop further execution
            }
            // Check if either student or instructor radio button is selected
            if (!signUp_student_rb.Checked && !signUp_instructor_rb.Checked)
            {
                MessageBox.Show("Please select the user type.");
                return;
            }
            string userType = signUp_student_rb.Checked ? "s" : "i";
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "INSERT INTO Users (UserId, Username, Email, Password, UserType) VALUES (:UserId, :Username, :Email, :Password, :UserType)";
            cmd.CommandType = CommandType.Text;
            int newId = GetNextUserId();
            cmd.Parameters.Add("UserId", newId);
            cmd.Parameters.Add("Username", username);
            cmd.Parameters.Add("Email", email);
            cmd.Parameters.Add("Password", password);
            cmd.Parameters.Add("UserType", userType);

            int r = cmd.ExecuteNonQuery();
            if (r != -1)
            {
                MessageBox.Show("User registered successfully!");
                SignIn_pnl.BringToFront();

            }
            ClearForm();

        }
        private int GetNextUserId()
        {
            // Query to retrieve the maximum ID from the User table
            string query = "SELECT MAX(UserId) FROM Users";

            OracleCommand cmd = new OracleCommand(query, conn);
            object result = cmd.ExecuteScalar();

            int nextUserId = 1; // Default value if no records exist

            if (result != null && result != DBNull.Value)
            {
                nextUserId = Convert.ToInt32(result) + 1;
            }

            return nextUserId;
        }
        

        private void admin_home_btn_Click(object sender, EventArgs e)
        {
            Admin_page_pnl.BringToFront();
        }

        private void admin_reports_btn_Click(object sender, EventArgs e)
        {
            Reports_pnl.BringToFront();
        }

        private void reports_screen_home_btn_Click(object sender, EventArgs e)
        {
            Admin_page_pnl.BringToFront();
        }

        private void cr_courses_info_btn_Click(object sender, EventArgs e)
        {
            courses_rprt_pnl.BringToFront();
        }

        private void cr_students_prgrs_btn_Click(object sender, EventArgs e)
        {
            students_progress_crystalReport.ReportSource = SCR;
            courses_rprt_pnl.SendToBack();
        }

        private void admin_load_btn_Click(object sender, EventArgs e)
        {

            string ordb = "Data Source= orcl ; User Id = hr; Password = hr;";
            string cmdstr = "";
            if (!Manage_Accounts.Checked && !Manage_Courses.Checked && !Show_inactive_users.Checked && !Show_Pending_Courses.Checked)
            {
                MessageBox.Show("Please select an option");
            }
            else
            {
                if (Manage_Accounts.Checked)
                    cmdstr = "select * from users where usertype in ('i','s')";
                else if (Manage_Courses.Checked)
                    cmdstr = "select * from course";
                else if (Show_inactive_users.Checked)
                    cmdstr = "select * from users where isactive in ('n') ";
                else if (Show_Pending_Courses.Checked)
                    cmdstr = "select * from course where Status= 'pending'";

                adapter = new OracleDataAdapter(cmdstr, ordb);
                ds = new DataSet();
                adapter.Fill(ds);
                dataGridView1.DataSource = ds.Tables[0];
            }
        }

        private void save_changes_Click(object sender, EventArgs e)
        {
            builder = new OracleCommandBuilder(adapter);
            adapter.Update(ds.Tables[0]);
        }

        private void Registration_form_Load(object sender, EventArgs e)
        {
            CCR = new Courses_CrystalReport();
            SCR = new Students_CrystalReport();

            foreach (ParameterDiscreteValue v in CCR.ParameterFields[0].DefaultValues)
                subjects_cmbx.Items.Add(v.Value);

            conn = new OracleConnection(ordb);
            conn.Open();

            // Get the next course ID and set it in the ID text box
            int nextCourseId = GetNextCourseId();
            txt_Id.Text = nextCourseId.ToString();

            CategoriesComboBox();
        }

        private void show_selected_sbj_btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(subjects_cmbx.Text)) {
                MessageBox.Show("Please choose a Subject.");
            }
            else
            {
                CCR.SetParameterValue(0, subjects_cmbx.Text);
                course_details_crystalReport.ReportSource = CCR;
            }
        }

        private void show_prgrs_btn_Click(object sender, EventArgs e)
        {
            student_show_prgrs_pnl.BringToFront();
        }

        private void istructor_manage_courses_btn_Click(object sender, EventArgs e)
        {
            instructor_manage_courses_pnl.BringToFront();
        }

        private void instructor_add_course_btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_Id.Text) || string.IsNullOrWhiteSpace(txt_Title.Text) || string.IsNullOrWhiteSpace(txt_desc.Text) ||
        string.IsNullOrWhiteSpace(txt_Subj.Text) || string.IsNullOrWhiteSpace(txt_Uni.Text) || string.IsNullOrWhiteSpace(txt_Inst.Text)) {
                MessageBox.Show("Please fill in all the fields.");
            }
            else
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;
                cmd.CommandText = "insert into Course values(:CourseId,:Title,:Description,:Subject,:University,:Instructor)";
                cmd.Parameters.Add("CourseId", int.Parse(txt_Id.Text));
                cmd.Parameters.Add("Title", txt_Title.Text);
                cmd.Parameters.Add("Description", txt_desc.Text);
                cmd.Parameters.Add("Subject", txt_Subj.Text);
                cmd.Parameters.Add("University", txt_Uni.Text);
                cmd.Parameters.Add("Instructor", txt_Inst.Text);
                //int NextCourseId = GetNextCourseId();
                //cmd.Parameters.Add("CourseId", NextCourseId);
                int r = cmd.ExecuteNonQuery();
                if (r != -1)
                {
                    MessageBox.Show("New Course is Added");
                    // Increment the ID and update the ID text box
                    int nextCourseId = int.Parse(txt_Id.Text) + 1;
                    txt_Id.Text = nextCourseId.ToString();
                }
                ClearForm();
            }
        }

        private int GetNextCourseId()
        {
            // Query to retrieve the maximum ID from the Course table
            string query = "SELECT MAX(CourseId) FROM Course";

            OracleCommand cmd = new OracleCommand(query, conn);
            object result = cmd.ExecuteScalar();

            int nextId = 1; // Default value if no records exist

            if (result != null && result != DBNull.Value)
            {
                nextId = Convert.ToInt32(result) + 1;
            }

            return nextId;
        }

        private void ClearForm()
        {

            txt_Title.Text = string.Empty;
            txt_desc.Text = string.Empty;
            txt_Subj.Text = string.Empty;
            txt_Uni.Text = string.Empty;
            txt_Inst.Text = string.Empty;
        }

        private void Show_btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_userId.Text))
            {
                MessageBox.Show("Please enter user ID");
            }
            else
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;
                cmd.CommandText = "GetCourseProgress";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("UserId", txt_userId.Text);
                cmd.Parameters.Add("Progress", OracleDbType.RefCursor, ParameterDirection.Output);
                OracleDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    string progressId = dr.GetString(dr.GetOrdinal("ProgressID")); // Assuming column name is "courseId"
                    string completedAss = dr.GetString(dr.GetOrdinal("CompletedAssignments")); // Column name "completedAss"
                    string upcomingAss = dr.GetString(dr.GetOrdinal("UpcomingAssignments")); // Column name "UpcomingAss"
                    string viewedLec = dr.GetString(dr.GetOrdinal("ViewedLectures")); // Column name "ViewedLec"

                    string courseId = progressId.Split('_')[1];

                    //Create a message box for each row
                    string message = string.Format("Course ID: {0}\nCompleted Assignments: {1}\nUpcoming Assignments: {2}\nViewed Lectures: {3}", courseId, completedAss, upcomingAss, viewedLec);


                    MessageBox.Show(message);
                    // "Course Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                dr.Close(); // Close the data reader
            }
        }

        private void CategoriesComboBox()
        {
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT DISTINCT Subject FROM Course";
            cmd.CommandType = CommandType.Text;

            OracleDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                string category = dr["Subject"].ToString();
                Category_comboBox.Items.Add(category);
            }

            dr.Close();
        }

        private void Category_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "select * from Course where Subject = :category";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("category", Category_comboBox.SelectedItem.ToString());

            OracleDataReader dr = cmd.ExecuteReader();

            DataTable dataTable = new DataTable();
            dataTable.Load(dr);

            dataGridView2.DataSource = dataTable;
            dr.Close();
        }


    }
}
