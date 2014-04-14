using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;

namespace MobileSurveillanceWebApplication.Utility
{
    /// <summary>
    /// Data Access Helper Class
    /// </summary>
    class DataAccessHelper
    {

        private readonly OleDbConnection connection;

        /// <summary>
        /// Connection string
        /// </summary>
        private string connectionString;

        /// <summary>
        /// Constructor
        /// </summary>
        public DataAccessHelper(string connectionString)
        {
            this.connection = new OleDbConnection(connectionString);
        }

        /// <summary>
        /// Open connection
        /// </summary>
        /// <returns></returns>
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (OleDbException ex)
            {
                Console.WriteLine("Exception - {0}: {1}", ex.ErrorCode, ex.Message);
            }
            catch (InvalidOperationException ioe)
            {
                Console.WriteLine(ioe.Message);
            }
            return false;
        }

        /// <summary>
        /// Close connection
        /// </summary>
        /// <returns></returns>
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Build OleDb command
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private OleDbCommand BuildCommand(string query, IEnumerable<IDataParameter> parameters)
        {
            if (this.OpenConnection())
            {
                var command = new OleDbCommand(query, connection);
                // Insert parameters
                if (parameters != null)
                {
                    foreach (OleDbParameter item in parameters)
                    {
                        if (/*item.IsNullable &&*/
                            (item.Value == null || (item.Value is string && string.IsNullOrEmpty(item.Value.ToString()))))
                        {
                            item.Value = DBNull.Value;
                        }
                        command.Parameters.Add(item);
                    }
                }
                return command;
            }
            return null;
        }

        /// <summary>
        /// Execute a stored-procedure and return IDataReader
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string query, IDataParameter[] parameters)
        {
            IDataReader result = null;
            try
            {
                var cmd = BuildCommand(query, parameters);
                if (cmd != null)
                {
                    result = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    //if (parameters != null)
                    //{
                    //    foreach (IDataParameter para in parameters.Where(t => t.Direction == ParameterDirection.Output ||
                    //                                                       t.Direction == ParameterDirection.InputOutput ||
                    //                                                       t.Direction == ParameterDirection.ReturnValue))
                    //    {
                    //        para.Value = cmd.Parameters[para.ParameterName].Value;
                    //    }
                    //}
                }
            }
            catch
            {
                if (result != null)
                {
                    result.Close();
                }
                throw;
            }
            return result;
        }

        /// <summary>
        /// Execute scalar
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string query, IDataParameter[] parameters)
        {
            object retVal = null;
            using (var command = BuildCommand(query, parameters))
            {
                if(command != null)
                {
                    retVal = command.ExecuteScalar();
                    //if (parameters != null)
                    //{
                    //    foreach (IDataParameter para in parameters.Where(t => t.Direction == ParameterDirection.Output ||
                    //                                                          t.Direction == ParameterDirection.ReturnValue))
                    //    {
                    //        para.Value = command.Parameters[para.ParameterName].Value;
                    //    }
                    //}
                }
                this.CloseConnection();
            }
            return retVal;
        }

        /// <summary>
        /// Exectute non query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string query, IDataParameter[] parameters)
        {
            int retVal = 0;
            using (var command = BuildCommand(query, parameters))
            {
                if(command != null)
                {
                    retVal = command.ExecuteNonQuery();
                    //if (parameters != null)
                    //{
                    //    foreach (IDataParameter para in parameters.Where(t => t.Direction == ParameterDirection.Output ||
                    //                                                      t.Direction == ParameterDirection.ReturnValue))
                    //    {
                    //        para.Value = command.Parameters[para.ParameterName].Value;
                    //    }
                    //}
                }
                this.CloseConnection();
            }
            return retVal;
        }
    }
}
