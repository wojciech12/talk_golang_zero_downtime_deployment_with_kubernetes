###########
Demo DotNet
###########

Docker
------

::

  $(make docker_run_echo)
  docker logs zero-kube-demo-net -f
  docker stop zero-kube-demo-net
  docker stop --time=10 zero-kube-demo-net

::

  watch -n0.3 -x curl -s -I curl 127.0.0.1:8090/ready
